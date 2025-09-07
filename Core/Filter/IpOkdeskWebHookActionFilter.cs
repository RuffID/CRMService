using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.Extensions.Options;
using CRMService.Models.ConfigClass;

namespace CRMService.Core.Filter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class IpOkdeskWebHookActionFilterAttribute : ActionFilterAttribute
    {
        private readonly ILogger<IpOkdeskWebHookActionFilterAttribute> _logger;
        private readonly List<(IPAddress Network, int PrefixLength)> _cidrWhitelist = [];
        private readonly List<IPAddress> _singleIpWhitelist = [];

        public IpOkdeskWebHookActionFilterAttribute(ILoggerFactory logger, IOptions<WebHookOkdeskOptions> webHookOptions)
        {
            _logger = logger.CreateLogger<IpOkdeskWebHookActionFilterAttribute>();
            string[] ipList = webHookOptions.Value.IpAddressList;

            // Запись ip адресов из конфига в переменную для дальнейшей работы
            foreach (var ip in ipList)
            {
                // Если указан ip с маской подсети (CIDR запись), то разбивает с помощью '/' символа
                if (ip.Contains('/'))
                {
                    string[]? parts = ip.Split('/');
                    IPAddress? network = IPAddress.Parse(parts[0]);
                    int prefixLength = int.Parse(parts[1]);

                    _cidrWhitelist.Add((network, prefixLength));
                }
                // Если указан обычный IP
                else
                {
                    _singleIpWhitelist.Add(IPAddress.Parse(ip));
                }
            }

            // Добавление localhost (IPv4 и IPv6)
            _singleIpWhitelist.Add(IPAddress.Loopback);        // 127.0.0.1
            _singleIpWhitelist.Add(IPAddress.IPv6Loopback);    // ::1
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Получение ip адреса из запроса
            var remoteIp = context.HttpContext.Connection.RemoteIpAddress;
            // Нужно для дебага
            var forwarderIp = context.HttpContext.Request.Headers["X-Forwarder-For"].ToString();

            // Если ip не в белом списке, то выдаёт код ошибки авторизации
            if (!IsAllowed(remoteIp))
            {
                _logger.LogWarning("Forbidden Request from Remote IP address: {RemoteIp}, forwader ip: {ForwarderIpFor}", remoteIp, forwarderIp);
                context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
                return;
            }

            base.OnActionExecuting(context);
        }

        private bool IsAllowed(IPAddress? ip)
        {
            if (ip == null)
                return false;

            // Проверка по точному IP
            if (_singleIpWhitelist.Contains(ip))
                return true;

            // Проверка по маскам CIDR
            foreach (var (network, prefixLength) in _cidrWhitelist)
            {
                if (IsInCidrRange(ip, network, prefixLength))
                    return true;
            }

            return false;
        }

        private static bool IsInCidrRange(IPAddress address, IPAddress network, int prefixLength)
        {
            // Проверка совместимости адресов (IPv4 vs IPv6)
            // Если у одного адреса IPv4, а у другого IPv6 — сразу false, они несовместимы, здесь проверяется только IPv4 адреса.
            if (address.AddressFamily != network.AddressFamily)
                return false;

            // Получаем байтовые представления адреса и сети
            // Оба IP (адрес и сеть) переводятся в массивы байт для побитовых операций.
            var addressBytes = address.GetAddressBytes();
            var networkBytes = network.GetAddressBytes();

            // Определяем, сколько полных байтов покрывает маска
            // Например, при /24 — это 3 полных байта (24 / 8 = 3).
            int fullBytes = prefixLength / 8;

            // Определяем, сколько бит в последнем байте (если маска не кратна 8)
            // Например, для /25 это будет 1 оставшийся бит (25 % 8 = 1).
            int remainingBits = prefixLength % 8;

            // Сравниваем полные байты (если маска >= 8)
            // Если какой-то байт не совпадает, сразу false.
            for (int i = 0; i < fullBytes; i++)
            {
                if (addressBytes[i] != networkBytes[i])
                    return false;
            }

            // Обработка остаточных битов (если маска не кратна 8)            
            if (remainingBits > 0)
            {
                // Создается битовая маска для последнего байта.
                // Например, для оставшихся 3 битов маска будет: 11100000 (0xE0).
                /* 0xFF — это шестнадцатеричная запись числа. 
                 * Используется для создания битовой маски, которая позволяет проверить только часть последнего байта, когда маска не делится на 8. 
                 * В двоичной системе это: 11111111.
                 * В десятичной системе это: 255
                 */
                int mask = (byte)(0xFF << (8 - remainingBits));

                // Сравниваются только значимые (левые) биты в последнем байте.
                // Если не совпадают — возвращает false.
                if ((addressBytes[fullBytes] & mask) != (networkBytes[fullBytes] & mask))
                    return false;
            }

            // Если все проверки пройдены, адрес подходит
            return true;
        }
    }
}
