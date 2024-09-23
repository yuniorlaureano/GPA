using GPA.Utils.Permissions;

namespace GPA.Utils.Profiles
{
    public class ProfileConstants
    {
        public static List<Profile> MasterProfile { get; set; } = new List<Profile>
        {
            new Profile
            {
                App = Apps.GPA,
                Modules = new List<Module>
                {
                    InventoryModule(),
                    InvoiceModule(),
                    SecurityModule(),
                    GeneralModule(),
                    ReportModule(),
                }
            }
        };

        public static Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, bool>>>> InlineMasterProfile(
                List<Profile> assignedProfile
            )
        {
            var inlineMasterProfile = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, bool>>>>();
            var assignedProfileAsDictionary = ProfileToInlineDictionaryh(assignedProfile);

            foreach (var profile in MasterProfile)
            {
                if (!inlineMasterProfile.ContainsKey(profile.App))
                {
                    inlineMasterProfile.Add(profile.App, new Dictionary<string, Dictionary<string, Dictionary<string, bool>>>());

                }

                foreach (var module in profile.Modules)
                {
                    if (!inlineMasterProfile[profile.App].ContainsKey(module.Id))
                    {
                        inlineMasterProfile[profile.App].Add(module.Id, new Dictionary<string, Dictionary<string, bool>>());
                    }

                    foreach (var component in module.Components)
                    {
                        if (!inlineMasterProfile[profile.App][module.Id].ContainsKey(component.Id))
                        {
                            inlineMasterProfile[profile.App][module.Id].Add(component.Id, new Dictionary<string, bool>());
                        }
                        foreach (var permission in component.Permissions)
                        {
                            var valid = ContainPermission(profile.App, module.Id, component.Id, permission, assignedProfileAsDictionary);
                            inlineMasterProfile[profile.App][module.Id][component.Id].Add(permission, valid);
                        }
                    }
                }
            }

            return inlineMasterProfile;
        }

        private static bool ContainPermission(
            string app,
            string module,
            string component,
            string permission,
            Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, bool>>>> profileToVerify
            )
        {
            return profileToVerify.ContainsKey(app) &&
                profileToVerify[app].ContainsKey(module) &&
                profileToVerify[app][module].ContainsKey(component) &&
                profileToVerify[app][module][component].ContainsKey(permission);
        }

        public static Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, bool>>>> ProfileToInlineDictionaryh(List<Profile> profiles)
        {
            var inlineMasterProfile = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, bool>>>>();
            foreach (var profile in profiles)
            {
                if (!inlineMasterProfile.ContainsKey(profile.App))
                {
                    inlineMasterProfile.Add(profile.App, new Dictionary<string, Dictionary<string, Dictionary<string, bool>>>());

                }

                foreach (var module in profile.Modules)
                {
                    if (!inlineMasterProfile[profile.App].ContainsKey(module.Id))
                    {
                        inlineMasterProfile[profile.App].Add(module.Id, new Dictionary<string, Dictionary<string, bool>>());
                    }

                    foreach (var component in module.Components)
                    {
                        if (!inlineMasterProfile[profile.App][module.Id].ContainsKey(component.Id))
                        {
                            inlineMasterProfile[profile.App][module.Id].Add(component.Id, new Dictionary<string, bool>());
                        }
                        foreach (var permission in component.Permissions)
                        {
                            inlineMasterProfile[profile.App][module.Id][component.Id].Add(permission, true);
                        }
                    }
                }
            }

            return inlineMasterProfile;
        }

        public static PermissionPathWithValue CreatePath(string app, string module, string component, string valueToCompare)
        {
            var path = new PermissionPathWithValue();

            path.PermissionPath.Add(new PathStep { PropertyName = "app", ArrayPropertyValue = app });

            path.PermissionPath.Add(new PathStep { PropertyName = "modules" });
            path.PermissionPath.Add(new PathStep { PropertyName = "id", ArrayPropertyValue = module });

            path.PermissionPath.Add(new PathStep { PropertyName = "components" });
            path.PermissionPath.Add(new PathStep { PropertyName = "id", ArrayPropertyValue = component });

            path.PermissionPath.Add(new PathStep { PropertyName = "permissions" });
            path.PermissionPath.Add(new PathStep { PropertyName = "permissions", IsSimpleArray = true });

            path.ValueToCompare = valueToCompare;

            return path;
        }

        private static Module InventoryModule()
        {
            return new Module
            {
                Id = Modules.Inventory,
                Components = new List<Component>
                        {
                            new Component
                            {
                                Id = Components.Addon,
                                Permissions = new List<string>
                                {
                                    Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read
                                }
                            },
                            new Component
                            {
                                Id = Components.Category,
                                Permissions = new List<string>
                                {
                                    Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read
                                }
                            },
                            new Component
                            {
                                Id = Components.ProductLocation,
                                Permissions = new List<string>
                                {
                                    Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read
                                }
                            },
                            new Component
                            {
                                Id = Components.Product,
                                Permissions = new List<string>
                                {
                                    Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read
                                }
                            },
                            new Component
                            {
                                Id = Components.Provider,
                                Permissions = new List<string>
                                {
                                    Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read
                                }
                            },
                            new Component
                            {
                                Id = Components.Reason,
                                Permissions = new List<string>
                                {
                                    Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read
                                }
                            },
                            new Component
                            {
                                Id = Components.StockCycle,
                                Permissions = new List<string>
                                {
                                    Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read, Permissions.Open, Permissions.Close
                                }
                            },
                            new Component
                            {
                                Id = Components.Stock,
                                Permissions = new List<string>
                                {
                                    Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read, Permissions.ReadProducts, Permissions.ReadExistence, Permissions.RegisterInput, Permissions.RegisterOutput, Permissions.UpdateInput, Permissions.UpdateOutput, Permissions.Cancel, Permissions.ReadTransactions, Permissions.Upload, Permissions.Download
                                }
                            }
                        }
            };
        }

        private static Module InvoiceModule()
        {
            return new Module
            {
                Id = Modules.Invoice,
                Components = new List<Component>
                        {
                            new Component
                            {
                                Id = Components.Client,
                                Permissions = new List<string>
                                {
                                    Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read
                                }
                            },
                            new Component
                            {
                                Id = Components.Invoicing,
                                Permissions = new List<string>
                                {
                                    Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read, Permissions.Cancel, Permissions.Return, Permissions.Upload, Permissions.Download, Permissions.Print
                                }
                            },
                            new Component
                            {
                                Id = Components.ReceivableAccount,
                                Permissions = new List<string>
                                {
                                    Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read, Permissions.Print
                                }
                            }
                        }
            };
        }

        private static Module SecurityModule()
        {
            return new Module
            {
                Id = Modules.Security,
                Components = new List<Component>
                        {
                            new Component
                            {
                                Id = Components.User,
                                Permissions = new List<string>
                                {
                                    Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read, Permissions.Upload
                                }
                            },
                            new Component
                            {
                                Id = Components.Profile,
                                Permissions = new List<string>
                                {
                                    Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read, Permissions.AssignProfile, Permissions.UnAssignProfile
                                }
                            }
                        }
            };
        }

        private static Module ReportModule()
        {
            return new Module
            {
                Id = Modules.Reporting,
                Components = new List<Component>
                {
                    new Component
                    {
                        Id = Components.Report,
                        Permissions = new List<string>
                        {
                            Permissions.ExistenceReport, Permissions.StockCycleReport, Permissions.TransactionReport, Permissions.SaleReport
                        }
                    }
                }
            };
        }

        private static Module GeneralModule()
        {
            return new Module
            {
                Id = Modules.General,
                Components = new List<Component>
                {
                    new Component
                    {
                        Id = Components.Email,
                        Permissions = new List<string>
                        {
                            Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read, Permissions.Send
                        }
                    },
                    new Component
                    {
                        Id = Components.Auth,
                        Permissions = new List<string>
                        {
                            Permissions.UpdateUserProfile, Permissions.Upload
                        }
                    },
                    new Component
                    {
                        Id = Components.Blob,
                        Permissions = new List<string>
                        {
                            Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read, Permissions.Upload, Permissions.Download
                        }
                    },
                    new Component
                    {
                        Id = Components.PrintInformation,
                        Permissions = new List<string>
                        {
                            Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read, Permissions.Upload
                        }
                    },
                    new Component
                    {
                        Id = Components.Dashboard,
                        Permissions = new List<string>
                        {
                            Permissions.Read
                        }
                    },
                    new Component
                    {
                        Id = Components.Unit,
                        Permissions = new List<string>
                        {
                            Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read
                        }
                    }
                }
            };
        }

    }

    public class PermissionMessage
    {
        public string? Module { get; set; }
        public string? Component { get; set; }
        public string? Permission { get; set; }
        public string Message { get; set; }
    };

    public class Permissions
    {
        public const string Create = "create";
        public const string Update = "update";
        public const string Delete = "delete";
        public const string Read = "read";
        public const string ReadProducts = "read_products";
        public const string ReadExistence = "read_existence";
        public const string RegisterInput = "add_input";
        public const string RegisterOutput = "add_output";
        public const string UpdateInput = "update_input";
        public const string UpdateOutput = "update_output";
        public const string Open = "open";
        public const string Close = "close";
        public const string Cancel = "cancel";
        public const string AssignProfile = "assign_profile";
        public const string UnAssignProfile = "unAssign_profile";
        public const string UpdateUserProfile = "update_user_profile";
        public const string ReadTransactions = "read_transactions";
        public const string Return = "return";
        public const string Send = "send";
        public const string Upload = "upload";
        public const string Download = "download";
        public const string Print = "print";
        public const string ExistenceReport = "existence_report";
        public const string StockCycleReport = "stock_cycle_report";
        public const string TransactionReport = "transaction_report";
        public const string SaleReport = "sale_report";
    }

    public class Apps
    {
        public const string GPA = "GPA";
    }

    public class Modules
    {
        public const string Inventory = "inventory";
        public const string Invoice = "invoice";
        public const string Security = "security";
        public const string Reporting = "reporting";
        public const string General = "general";
    }

    public class Components
    {
        public const string Addon = "addon";
        public const string Category = "category";
        public const string ProductLocation = "productLocation";
        public const string Product = "product";
        public const string Provider = "provider";
        public const string Reason = "reason";
        public const string StockCycle = "stockCycle";
        public const string Stock = "stock";
        public const string Client = "client";
        public const string Invoicing = "invoicing";
        public const string ReceivableAccount = "receivable";
        public const string User = "user";
        public const string Profile = "profile";
        public const string Auth = "auth";
        public const string Email = "email";
        public const string Blob = "blob";
        public const string PrintInformation = "printInformation";
        public const string Dashboard = "dashboard";
        public const string Unit = "unit";
        public const string Report = "report";
    }

    public class PermissionsTranslate
    {
        public static Dictionary<string, string> Translates { get; set; } = new()
        {
          { Permissions.Create, "crear"},
          { Permissions.Update,"actualizar"},
          { Permissions.Delete,"eliminar"},
          { Permissions.Read,"leer"},
          { Permissions.ReadProducts,"leer productos"},
          { Permissions.ReadExistence,"leer existencia"},
          { Permissions.RegisterInput,"agregar entrada"},
          { Permissions.RegisterOutput,"agregar salida"},
          { Permissions.UpdateInput,"actualizar entrada"},
          { Permissions.UpdateOutput,"actualizar salida"},
          { Permissions.Open,"abrir"},
          { Permissions.Close,"cerrar"},
          { Permissions.Cancel,"cancelar"},
          { Permissions.AssignProfile,"asignar perfil"},
          { Permissions.UnAssignProfile,"desasignar perfil"},
          { Permissions.UpdateUserProfile,"actualizar perfil de usuario"},
          { Permissions.ReadTransactions,"lee transacciones"},
          { Permissions.Return,"devolver"},
          { Permissions.Send,"enviar email"},
          { Permissions.Upload,"subir archvio"},
          { Permissions.Download,"descargar archivo"},
          { Permissions.Print,"imprimir"},
          { Permissions.ExistenceReport,"reporte de existencia"},
          { Permissions.StockCycleReport,"reporte de ciclo de inventario"},
          { Permissions.TransactionReport,"reporte de transacciones"},
          { Permissions.SaleReport,"reporte de venta"},
          { Apps.GPA, "Sistema GPA"},
          { Modules.Inventory, "inventario"},
          { Modules.Invoice, "faturación"},
          { Modules.Security, "seguridad"},
          { Modules.Reporting, "reporte"},
          { Modules.General, "general"},
          { Components.Addon, "agregado"},
          { Components.Category, "categoría"},
          { Components.ProductLocation, "ubicación de producto"},
          { Components.Product, "producto"},
          { Components.Provider, "proveedor"},
          { Components.Reason, "razón"},
          { Components.StockCycle, "ciclo de inventario"},
          { Components.Stock, "inventario"},
          { Components.Client, "cliente"},
          { Components.Invoicing, "facturación"},
          { Components.ReceivableAccount, "cuentas por cobrar"},
          { Components.User, "usuario"},
          { Components.Profile, "perfil"},
          { Components.Auth, "autenticación"},
          { Components.Email, "correo"},
          { Components.Blob, "blob"},
          { Components.PrintInformation, "inforamción de impresión"},
          { Components.Dashboard, "dashboard"},
          { Components.Unit, "unidad"},
          { Components.Report, "reporte"}
        };
    }
}
