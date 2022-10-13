using EasyBuildSystem.Features.Scripts.Core.Base.Addon;
using EasyBuildSystem.Features.Scripts.Core.Base.Addon.Enums;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Core.Addons.Helper
{
    public class AddonHelper : MonoBehaviour
    {
        #region Methods

        public static List<AddonAttribute> GetAddons()
        {
            List<AddonAttribute> ResultAddons = new List<AddonAttribute>();

            Type[] ActiveBehaviours = GetAllSubTypes(typeof(MonoBehaviour));

            foreach (Type Type in ActiveBehaviours)
            {
                object[] Attributes = Type.GetCustomAttributes(typeof(AddonAttribute), false);

                if (Attributes != null)
                {
                    for (int i = 0; i < Attributes.Length; i++)
                    {
                        if ((AddonAttribute)Attributes[i] != null)
                        {
                            ((AddonAttribute)Attributes[i]).Behaviour = Type;

                            ResultAddons.Add((AddonAttribute)Attributes[i]);
                        }
                    }
                }
            }

            return ResultAddons;
        }

        public static List<AddonAttribute> GetAddonsByTarget(AddonTarget target)
        {
            List<AddonAttribute> ResultAddons = new List<AddonAttribute>();

            Type[] ActiveBehaviours = GetAllSubTypes(typeof(MonoBehaviour));

            foreach (Type Type in ActiveBehaviours)
            {
                object[] Attributes = Type.GetCustomAttributes(typeof(AddonAttribute), false);

                if (Attributes != null)
                {
                    for (int i = 0; i < Attributes.Length; i++)
                    {
                        if ((AddonAttribute)Attributes[i] != null)
                        {
                            if (((AddonAttribute)Attributes[i]).Target == target)
                            {
                                ((AddonAttribute)Attributes[i]).Behaviour = Type;

                                ResultAddons.Add((AddonAttribute)Attributes[i]);
                            }
                        }
                    }
                }
            }

            return ResultAddons;
        }

        public static Type[] GetAllSubTypes(Type aBaseClass)
        {
            List<Type> Result = new List<Type>();

            Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly Assembly in Assemblies)
            {
                Type[] Types = Assembly.GetTypes();

                foreach (Type T in Types)
                {
                    if (T.IsSubclassOf(aBaseClass))
                    {
                        Result.Add(T);
                    }
                }
            }

            return Result.ToArray();
        }

        #endregion Methods
    }
}