﻿using GPA.Utils.Permissions;

namespace GPA.Utils.Profiles
{
    public class PermissionMessage
    {
        public string Module { get; set; } = "";
        public string Component { get; set; } = "";
        public string Permission { get; set; } = "";
        public string Message { get; set; } = "";
    };

    public class Permissions
    {
        public const string Create = "create";
        public const string Update = "update";
        public const string Delete = "delete";
        public const string Read = "read";
        public const string AssignProfile = "assignProfile";
        public const string UnAssignProfile = "unAssignProfile";
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
        public const string Report = "report";
        public const string Common = "common";
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
        public const string Invoice = "invoice";
        public const string ReceivableAccount = "receivable";
        public const string User = "user";
        public const string Profile = "profile";
    }

    public class ProfileConstants
    {
        public static List<Profile> MasterProfile { get; set; } = new List<Profile>
        {
            new Profile
            {
                App = Apps.GPA,
                Modules = new List<Module>
                {
                    new Module
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
                                    Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read
                                }
                            },
                            new Component
                            {
                                Id = Components.Stock,
                                Permissions = new List<string>
                                {
                                    Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read
                                }
                            }
                        }
                    },
                    new Module
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
                                Id = Components.Invoice,
                                Permissions = new List<string>
                                {
                                    Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read
                                }
                            },
                            new Component
                            {
                                Id = Components.ReceivableAccount,
                                Permissions = new List<string>
                                {
                                    Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read
                                }
                            }
                        }
                    },
                    new Module
                    {
                        Id = Modules.Security,
                        Components = new List<Component>
                        {
                            new Component
                            {
                                Id = Components.User,
                                Permissions = new List<string>
                                {
                                    Permissions.Create, Permissions.Update, Permissions.Delete, Permissions.Read
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
                    },
                    new Module
                    {
                        Id = Modules.Report,
                        Components = new List<Component>
                        {
                            new Component
                            {
                                Id = "",
                                Permissions = new List<string>
                                {
                                }
                            }
                        }
                    },
                    new Module
                    {
                        Id = Modules.Common,
                        Components = new List<Component>
                        {
                            new Component
                            {
                                Id = "",
                                Permissions = new List<string>
                                {
                                }
                            }
                        }
                    }
                }
            }
        };

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
    }
}
