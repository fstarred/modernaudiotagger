using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModernAudioTagger.Provider
{
    public class UnityContainerProvider
    {
        private UnityContainerProvider()
        {
        }

        static IUnityContainer UnityContainer;

        static UnityContainerProvider()
        {
            UnityContainer = new UnityContainer();
        }

        public static IUnityContainer Instance
        {
            get
            {
                return UnityContainer;
            }
        }
    }
}
