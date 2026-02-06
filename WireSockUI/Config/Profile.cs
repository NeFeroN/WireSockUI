using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WireSockUI.Extensions;
using WireSockUI.Native;
using static WireSockUI.Native.WireguardConfigParser;

namespace WireSockUI.Config
{
    /// <summary>
    ///     WireGuard Profile, including WireSock extensions
    /// </summary>
    internal class Profile
    {
        private string _address;

        // WireSock Extensions
        private string _allowedIPs;
        private string _dns;
        private string _endpoint;
        private string _listenport;
        private string _mtu;
        private string _persistentKeepAlive;

        private string _presharedKey;

        // Interface values
        private string _privateKey;

        // Peer values
        private string _publicKey;
        private string _socks5Proxy;

        // AmneziaWG parameters
        private string _jc;
        private string _jmin;
        private string _jmax;
        private string _s1;
        private string _s2;
        private string _s3;
        private string _s4;
        private string _h1;
        private string _h2;
        private string _h3;
        private string _h4;
        private string _i1;
        private string _i2;
        private string _i3;
        private string _i4;
        private string _i5;

        /// <summary>
        ///     Create an empty profile from scratch
        /// </summary>
        public Profile()
        {
        }

        /// <summary>
        ///     Load a profile from specified filepath
        /// </summary>
        /// <param name="profilePath">Full filepath to a profile</param>
        public Profile(string profilePath)
        {
            if (!File.Exists(profilePath))
                throw new FileNotFoundException($"Profile {Path.GetFileName(profilePath)} does not exist.");

            var parser = new ConfigParser(profilePath);
            var sections = parser.GetSectionNames();

            var configESections = sections as string[] ?? sections.ToArray();
            if (!configESections.Contains("Interface"))
                throw new ArgumentException(
                    $"Profile {Path.GetFileName(profilePath)} does not contain an \"Interface\" section.");

            var section = parser.GetSection("Interface");

            // Validate minimum required fields
            if (!section.ContainsKey("PrivateKey"))
                throw new ArgumentException(
                    $"Profile {Path.GetFileName(profilePath)}, section \"Interface\" does not have a \"PrivateKey\" defined.");

            if (!section.ContainsKey("Address"))
                throw new ArgumentException(
                    $"Profile {Path.GetFileName(profilePath)}, section \"Interface\" does not have a \"Address\" defined.");

            PrivateKey = section.Get("PrivateKey");
            Address = section.Get("Address");
            Dns = section.Get("DNS");
            Mtu = section.Get("MTU");

            // AmneziaWG parameters (optional)
            Jc = section.Get("Jc");
            Jmin = section.Get("Jmin");
            Jmax = section.Get("Jmax");
            S1 = section.Get("S1");
            S2 = section.Get("S2");
            S3 = section.Get("S3");
            S4 = section.Get("S4");
            H1 = section.Get("H1");
            H2 = section.Get("H2");
            H3 = section.Get("H3");
            H4 = section.Get("H4");
            I1 = section.Get("I1");
            I2 = section.Get("I2");
            I3 = section.Get("I3");
            I4 = section.Get("I4");
            I5 = section.Get("I5");

            if (!configESections.Contains("Peer"))
                throw new ArgumentException(
                    $"Profile {Path.GetFileName(profilePath)} does not contain an \"Peer\" section.");

            section = parser.GetSection("Peer");

            // Validate minimum required fields
            if (!section.ContainsKey("PublicKey"))
                throw new ArgumentException(
                    $"Profile {Path.GetFileName(profilePath)}, section \"Peer\" does not have a \"PublicKey\" defined.");

            if (!section.ContainsKey("Endpoint"))
                throw new ArgumentException(
                    $"Profile {Path.GetFileName(profilePath)}, section \"Peer\" does not have a \"Endpoint\" defined.");

            PeerKey = section.Get("PublicKey");
            PresharedKey = section.Get("PresharedKey");
            AllowedIPs = section.Get("AllowedIPs");
            Endpoint = section.Get("Endpoint");
            PersistentKeepAlive = section.Get("PersistentKeepAlive");

            AllowedApps = section.Get("AllowedApps");
            DisallowedApps = section.Get("DisallowedApps");
            DisallowedIPs = section.Get("DisallowedIPs");
            Socks5Proxy = section.Get("Socks5Proxy");
            Socks5ProxyUsername = section.Get("Socks5Username");
            Socks5ProxyPassword = section.Get("Socks5ProxyPassword");
        }

        /// <summary>
        ///     Local interface private key
        /// </summary>
        public string PrivateKey
        {
            get => _privateKey;
            set
            {
                ValidateKey("Interface", "PrivateKey", value);
                _privateKey = value;
            }
        }

        /// <summary>
        ///     Local interface public key, derived from private key
        /// </summary>
        public string PublicKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_privateKey))
                    // Determine public key from private key data
                    return
                        Convert.ToBase64String(
                            Curve25519.GetPublicKey(
                                Convert.FromBase64String(PrivateKey)));

                return null;
            }
        }

        /// <summary>
        ///     List of interface IP addresses
        /// </summary>
        public string Address
        {
            get => _address;
            set
            {
                ValidateAddresses("Interface", "Address", value, IpHelper.IsValidSubnetOrSingleIpAddress);
                _address = value;
            }
        }

        /// <summary>
        ///     List of interface DNS servers
        /// </summary>
        public string Dns
        {
            get => _dns;
            set
            {
                ValidateAddresses("Interface", "DNS", value, IpHelper.IsValidIpAddress);
                _dns = value;
            }
        }

        /// <summary>
        ///     Interface Maximum Transmissible Unit size
        /// </summary>
        public string Mtu
        {
            get => _mtu;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (!int.TryParse(value, out var mtu))
                        throw new FormatException("\"MTU\" in \"Interface\", is not a numerical value.");

                    if (mtu < 576 || mtu > 65535)
                        throw new FormatException("\"MTU\" in \"Interface\", invalid value. Expected 576...65535.");

                    _mtu = value;
                }
                else
                {
                    _mtu = null;
                }
            }
        }

        /// <summary>
        ///     Interface ListenPort
        /// </summary>
        public string ListenPort
        {
            get => _listenport;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (!int.TryParse(value, out var listenPort))
                        throw new FormatException("\"ListenPort\" in \"Interface\", is not a numerical value.");

                    if (listenPort < 1 || listenPort > 65535)
                        throw new FormatException(
                            "\"ListenPort\" in \"Interface\", invalid value. Expected 1...65535.");

                    _listenport = value;
                }
                else
                {
                    _listenport = null;
                }
            }
        }

        /// <summary>
        ///     Peer public key
        /// </summary>
        public string PeerKey
        {
            get => _publicKey;
            set
            {
                ValidateKey("Peer", "PublicKey", value);
                _publicKey = value;
            }
        }

        /// <summary>
        ///     Peer preshared key (optional)
        /// </summary>
        public string PresharedKey
        {
            get => _presharedKey;
            set
            {
                ValidateKey("Peer", "PresharedKey", value);
                _presharedKey = value;
            }
        }

        /// <summary>
        ///     Peer allowed IP list
        /// </summary>
        public string AllowedIPs
        {
            get => _allowedIPs;
            set
            {
                ValidateAddresses("Peer", "AllowedIPs", value, IpHelper.IsValidIpNetwork);
                _allowedIPs = value;
            }
        }

        /// <summary>
        ///     Peer endpoint address (DNS or IP)
        /// </summary>
        public string Endpoint
        {
            get => _endpoint;
            set
            {
                if (!IpHelper.IsValidAddress(value))
                    throw new FormatException("\"Endpoint\" in \"Peer\", is not a valid IPv4, IPv6 or domain address.");

                _endpoint = value;
            }
        }


        /// <summary>
        ///     Persistent keep alive interval
        /// </summary>
        public string PersistentKeepAlive
        {
            get => _persistentKeepAlive;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (!int.TryParse(value, out var mtu))
                        throw new FormatException("\"PersistentKeepalive\" in \"Peer\", is not a numerical value.");

                    if (mtu < 0 || mtu > 65535)
                        throw new FormatException(
                            "\"PersistentKeepalive\" in \"Peer\", invalid value. Expected 0...65535.");

                    _persistentKeepAlive = value;
                }
                else
                {
                    _persistentKeepAlive = null;
                }
            }
        }

        /// <summary>
        ///     Peer allowed applications list
        /// </summary>
        /// <remarks>WireSock specific extension</remarks>
        public string AllowedApps { get; set; }

        /// <summary>
        ///     Peer disallowed applications list
        /// </summary>
        /// <remarks>WireSock specific extension</remarks>
        public string DisallowedApps { get; set; }

        /// <summary>
        ///     Peer disallowed IP addresses
        /// </summary>
        /// <remarks>WireSock specific extension</remarks>
        public string DisallowedIPs { get; set; }

        /// <summary>
        ///     Peer SOCKS5 proxy
        /// </summary>
        /// <remarks>WireSock specific extension</remarks>
        public string Socks5Proxy
        {
            get => _socks5Proxy;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (!IpHelper.IsValidAddress(value))
                        throw new FormatException(
                            "\"Endpoint\" in \"Peer\", is not a valid IPv4, IPv6 or domain address.");

                    _socks5Proxy = value;
                }
                else
                {
                    _socks5Proxy = null;
                }
            }
        }

        /// <summary>
        ///     Peer SOCKS5 proxy username
        /// </summary>
        /// <remarks>WireSock specific extension</remarks>
        public string Socks5ProxyUsername { get; set; }

        /// <summary>
        ///     Peer SOCKS5 proxy password
        /// </summary>
        /// <remarks>WireSock specific extension</remarks>
        public string Socks5ProxyPassword { get; set; }

        /// <summary>
        ///     AmneziaWG junk packet count
        /// </summary>
        /// <remarks>AmneziaWG specific parameter</remarks>
        public string Jc
        {
            get => _jc;
            set => _jc = value;
        }

        /// <summary>
        ///     AmneziaWG junk packet minimum size
        /// </summary>
        /// <remarks>AmneziaWG specific parameter</remarks>
        public string Jmin
        {
            get => _jmin;
            set => _jmin = value;
        }

        /// <summary>
        ///     AmneziaWG junk packet maximum size
        /// </summary>
        /// <remarks>AmneziaWG specific parameter</remarks>
        public string Jmax
        {
            get => _jmax;
            set => _jmax = value;
        }

        /// <summary>
        ///     AmneziaWG init packet junk size 1
        /// </summary>
        /// <remarks>AmneziaWG specific parameter</remarks>
        public string S1
        {
            get => _s1;
            set => _s1 = value;
        }

        /// <summary>
        ///     AmneziaWG init packet junk size 2
        /// </summary>
        /// <remarks>AmneziaWG specific parameter</remarks>
        public string S2
        {
            get => _s2;
            set => _s2 = value;
        }

        /// <summary>
        ///     AmneziaWG init packet junk size 3
        /// </summary>
        /// <remarks>AmneziaWG specific parameter</remarks>
        public string S3
        {
            get => _s3;
            set => _s3 = value;
        }

        /// <summary>
        ///     AmneziaWG init packet junk size 4
        /// </summary>
        /// <remarks>AmneziaWG specific parameter</remarks>
        public string S4
        {
            get => _s4;
            set => _s4 = value;
        }

        /// <summary>
        ///     AmneziaWG header 1 (can be range like "664627419-2055816503")
        /// </summary>
        /// <remarks>AmneziaWG specific parameter</remarks>
        public string H1
        {
            get => _h1;
            set => _h1 = value;
        }

        /// <summary>
        ///     AmneziaWG header 2 (can be range like "2055998124-2115259558")
        /// </summary>
        /// <remarks>AmneziaWG specific parameter</remarks>
        public string H2
        {
            get => _h2;
            set => _h2 = value;
        }

        /// <summary>
        ///     AmneziaWG header 3 (can be range like "2129959140-2136758940")
        /// </summary>
        /// <remarks>AmneziaWG specific parameter</remarks>
        public string H3
        {
            get => _h3;
            set => _h3 = value;
        }

        /// <summary>
        ///     AmneziaWG header 4 (can be range like "2141503360-2144649799")
        /// </summary>
        /// <remarks>AmneziaWG specific parameter</remarks>
        public string H4
        {
            get => _h4;
            set => _h4 = value;
        }

        /// <summary>
        ///     AmneziaWG init packet data 1
        /// </summary>
        /// <remarks>AmneziaWG specific parameter</remarks>
        public string I1
        {
            get => _i1;
            set => _i1 = value;
        }

        /// <summary>
        ///     AmneziaWG init packet data 2
        /// </summary>
        /// <remarks>AmneziaWG specific parameter</remarks>
        public string I2
        {
            get => _i2;
            set => _i2 = value;
        }

        /// <summary>
        ///     AmneziaWG init packet data 3
        /// </summary>
        /// <remarks>AmneziaWG specific parameter</remarks>
        public string I3
        {
            get => _i3;
            set => _i3 = value;
        }

        /// <summary>
        ///     AmneziaWG init packet data 4
        /// </summary>
        /// <remarks>AmneziaWG specific parameter</remarks>
        public string I4
        {
            get => _i4;
            set => _i4 = value;
        }

        /// <summary>
        ///     AmneziaWG init packet data 5
        /// </summary>
        /// <remarks>AmneziaWG specific parameter</remarks>
        public string I5
        {
            get => _i5;
            set => _i5 = value;
        }

        internal static void ValidateKey(string section, string key, string keyValue)
        {
            byte[] keyBinary;

            if (string.IsNullOrWhiteSpace(keyValue)) return;

            try
            {
                keyBinary = Convert.FromBase64String(keyValue);
            }
            catch (FormatException)
            {
                throw new FormatException($"\"{key}\" in \"{section}\", invalid base64 encoded value.");
            }

            // 256-bit keys only
            if (keyBinary.Length != 32)
                throw new FormatException(
                    $"\"{key}\" in \"{section}\", invalid key length, only 256-bit keys are supported.");
        }

        internal static void ValidateAddresses(string section, string key, string keyValue,
            Func<string, bool> validator)
        {
            if (string.IsNullOrWhiteSpace(keyValue)) return;

            foreach (var value in keyValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                if (!validator(value.Trim()))
                    throw new FormatException($"\"{key}\" in \"{section}\", invalid address \"{value}\".");
        }

        public static IEnumerable<string> GetProfiles()
        {
            var files = Directory.GetFiles(Global.ConfigsFolder);

            foreach (var file in files)
            {
                if (!file.EndsWith(".conf")) continue;

                yield return Path.GetFileNameWithoutExtension(file);
            }
        }

        /// <summary>
        ///     Retrieve the full path to a given <paramref name="profileName" />
        /// </summary>
        /// <param name="profileName">Profile name</param>
        /// <returns>Full path to the profile</returns>
        /// <remarks>The profile might not exist, this merely returns the path it should be at.</remarks>
        public static string GetProfilePath(string profileName)
        {
            return Path.Combine(Global.ConfigsFolder, profileName + ".conf");
        }

        /// <summary>
        ///     Load an existing named profile
        /// </summary>
        /// <param name="profileName">Profile name (i.e. filename without extension)</param>
        public static Profile LoadProfile(string profileName)
        {
            var filename = GetProfilePath(profileName);
            return new Profile(filename);
        }
    }
}