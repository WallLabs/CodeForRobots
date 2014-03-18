using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Dss.Runtime.Security;

namespace MrdsToolkit.Windows.Services
{
    /// <summary>
    /// Manages <see cref="SecuritySettings"/>.
    /// </summary>
    public static class SecurityManager
    {
        /// <summary>
        /// Creates default settings which allows only the service itself to communicate.
        /// </summary>
        public static SecuritySettings CreateDefault()
        {
            // Core settings require authentication and signed assemblies
            var security = new SecuritySettings
            {
                AuthenticationRequired = true,
                OnlySignedAssemblies = false
            };

            // Grant service host itself full access
            security.Paths.Add(new PathPermission
                {
                    PathSegment = "",   // Root paths must be empty
                    Users = new List<UserPermission>
                        {
                            new UserPermission
                                {
                                    Rights = DsspRights.All,
                                    SddlSid = "S-1-5-10",
                                    UserName = @"NT AUTHORITY\SELF",
                                    Inherit = true
                                },
                            new UserPermission
                                {
                                    Rights = DsspRights.All,
                                    SddlSid = "S-1-5-20",
                                    UserName = @"NT AUTHORITY\NETWORK SERVICE",
                                    Inherit = true
                                } 
                        },
                });

            // Create an administrator role with full access and local administrators as members
            security.Roles.Add(new Role
                {
                    Allowed = DsspRights.All,
                    Name = "Administrator",
                    Paths = new List<string> {"/"}, // Role paths must NOT be empty
                    Contracts = new List<string> {"*"},
                    Users = new List<RoleUser>
                        {
                            new RoleUser
                                {
                                    SddlSid = "S-1-5-32-544",
                                    UserName = @"BUILTIN\Administrators"
                                }
                        }
                });

            // Create an administrator role with full access and local administrators as members
            security.Roles.Add(new Role
            {
                Allowed = DsspRights.All,
                Name = "Users",
                Paths = new List<string> { "/" },
                Contracts = new List<string> { "*" },
                Users = new List<RoleUser>
                        {
                            new RoleUser
                                {
                                    SddlSid = "S-1-5-32-545",
                                    UserName = @"BUILTIN\Users"
                                },
                            new RoleUser
                                {
                                    SddlSid = "S-1-5-20",
                                    UserName = @"NT AUTHORITY\NETWORK SERVICE"
                                }
                        }
            });
            
            // Return result
            return security;
        }

        /// <summary>
        /// Serializes a <see cref="SecuritySettings"/> object to an XML string.
        /// </summary>
        public static string Serialize(SecuritySettings value)
        {
            var serializer = new XmlSerializer(typeof(SecuritySettings));
            using (var buffer = new MemoryStream())
            {
                var writeSettings = new XmlWriterSettings {Encoding = Encoding.UTF8, Indent = true};
                var writer = XmlWriter.Create(buffer, writeSettings);
                serializer.Serialize(writer, value);
                return Encoding.UTF8.GetString(buffer.ToArray());
            }
        }

        /// <summary>
        /// Deserializes a <see cref="SecuritySettings"/> object from an XML string.
        /// </summary>
        public static SecuritySettings Deserialize(string value)
        {
            var serializer = new XmlSerializer(typeof(SecuritySettings));
            using (var reader = new StringReader(value))
                return (SecuritySettings)serializer.Deserialize(reader);
        }
    }
}
