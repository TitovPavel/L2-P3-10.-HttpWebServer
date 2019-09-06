using HttpWebServer.BL;
using HttpWebServer.Contoller;
using HttpWebServer.DAL;
using HttpWebServer.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HttpWebServer.Models
{
    class Locator
    {
        public List<Pair> pairs = new List<Pair>();


        public void Register()
        {
            pairs.Add(new Pair(typeof(ILogger), typeof(Logger)));
            pairs.Add(new Pair(typeof(IParticipantsRepository), typeof(ParticipantsRepository)));
            pairs.Add(new Pair(typeof(IParticipantsService), typeof(ParticipantsService)));

        }

        public object Resolve(Type type)
        {

            Pair pair = pairs.FirstOrDefault<Pair>(x => x.TypeInterface == type);

            if (pair!=null)
            {
                if (pair.Obj == null)
                {
                    CreateObject(pair);
                }
                return pair.Obj;

            }
            
            return null;
        }

        private void CreateObject(Pair pair)
        {
            Type typeObject = pair.TypeObject;

            ConstructorInfo[] ctorArray = typeObject.GetConstructors();

            if (ctorArray.Length > 0)
            {
                ConstructorInfo ctor = ctorArray[0];
                ParameterInfo[] parametersCtor = ctor.GetParameters();
                if (parametersCtor.Length > 0)
                {
                    List<Object> listParam = new List<object>();
                    for (int i = 0; i < parametersCtor.Length; i++)
                    {
                        listParam.Add(Resolve(parametersCtor[i].ParameterType));

                    }
                    pair.Obj = ctor.Invoke(listParam.ToArray());

                }
                else
                {
                    pair.Obj = ctor.Invoke(new object[] { });
                }
            }
        }

        public object Resolve(string urlController)
        {
            Type controllerType = Type.GetType($"HttpWebServer.Contoller.{urlController}Controller", false, true);
            if(controllerType!=null)
            {
                Pair pair = pairs.FirstOrDefault<Pair>(x => x.TypeObject == controllerType);

                if (pair == null)
                {
                    pair = new Pair(controllerType);
                    CreateObject(pair);
                    pairs.Add(pair);
                }

                return pair.Obj;
            }
            else
            {
                return null;
            }

        }

    }

    class Pair
    {
        public Pair(Type typeInterface, Type typeObject)
        {
            TypeInterface = typeInterface;
            TypeObject = typeObject;
        }

        public Pair(Type typeObject)
        {
            TypeObject = typeObject;
        }

        public Type TypeInterface { get; set; }
        public Type TypeObject { get; set; }
        public Object Obj  { get; set; }

    }
}
