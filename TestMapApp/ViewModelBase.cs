using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace MariaSearch.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        ///<summary>
        /// Implementes refactoring-safe NotifyPropertyChanged calls using Lambda's.
        ///</summary>
        ///<param name="property">Lambda specifying property being updated.</param>
        ///<typeparam name="TProperty">
        /// Property type parameter(can usually be inferred from property parameter).
        /// </typeparam>
        protected void NotifyPropertyChanged<TProperty>
            (Expression<Func<TProperty>> property)
        {
            var propertyName = ExtractPropertyName(property);
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        ///<summary>
        /// Returns name of a property from a lamba.
        ///</summary>
        ///<param name="property"></param>
        ///<typeparam name="TProperty"></typeparam>
        ///<returns></returns>
        private static string ExtractPropertyName<TProperty>
            (Expression<Func<TProperty>> property)
        {
            var lambda = (LambdaExpression) property;
            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression) lambda.Body;
                memberExpression = (MemberExpression) unaryExpression.Operand;
            }
            else memberExpression = (MemberExpression) lambda.Body;

            return memberExpression.Member.Name;
        }

        public static string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression != null)
                return memberExpression.Member.Name;
            return "-";
        }
    }
}
