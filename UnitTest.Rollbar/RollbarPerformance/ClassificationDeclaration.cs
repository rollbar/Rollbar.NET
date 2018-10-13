namespace UnitTest.Rollbar.RollbarPerformance
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Reflection;

    public class ClassificationDeclaration
    {
        #region all the mechanics behind the declaration conversion into the classifiers array

        private static readonly PropertyInfo[] classifierProperties = null;

        static ClassificationDeclaration()
        {
            classifierProperties = typeof(ClassificationDeclaration)
                .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
        }

        public object[] GetAllClassifiers()
        {
            List<object> classifiers = 
                new List<object>(ClassificationDeclaration.classifierProperties.Length);

            foreach(var property in ClassificationDeclaration.classifierProperties)
            {
                classifiers.Add(property.GetValue(this));
            }

            return classifiers.ToArray();
        }

        #endregion  all the mechanics behind the declaration conversion into the classifiers array

        #region All the classifiers constituting this classification declaration

        public Method Method { get; set; }
        public MethodVariant MethodVariant { get; set; }
        public PayloadType PayloadType { get; set; }
        public PayloadSize PayloadSize { get; set; }

        #endregion All the classifiers constituting this classification declaration
    }


    public enum Method
    {
        Log,
    }

    public enum MethodVariant
    {
        NotApplicable,
        Async,
        Blocking,
    }

    public enum PayloadType
    {
        Message,
        Exception,
    }

    public enum PayloadSize
    {
        Small,
        Medium,
        Large,
    }

}
