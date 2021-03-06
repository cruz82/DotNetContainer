using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace DotNetContainer.Validation
{
    public class Validator<T>
    {
        private readonly T _item;

        internal Validator(T item)
        {
            _item = item;
        }

        public Validator<T> Validate<TException>(Func<T, bool> evaluator, string message) where TException : Exception
        {
            if (!evaluator(_item))
                throw (Exception)Activator.CreateInstance(typeof(TException), message);

            return this;
        }

        public Validator<T> Validate(Func<T, bool> evaluator, string message)
        {
            Validate<ArgumentException>(evaluator, message);

            return this;
        }

        public Validator<T> IsValidModel()
        {
            if (_item == null)
                throw new ArgumentNullException("Model must not be null.");
            if (!_item.GetType().GetTypeInfo().IsClass)
                throw new ArgumentException("Object is not a model.");

            var ctx = new ValidationContext(_item);
            Validator.ValidateObject(_item, ctx, true);

            return this;
        }

        public Validator<T> HasValidProperty<TValue>(Expression<Func<T, TValue>> selector)
        {
            var ctx = new ValidationContext(_item);

            var expression = (MemberExpression)selector.Body;
            ctx.MemberName = expression.Member.Name;
            var val = selector.Compile()(_item);

            Validator.ValidateProperty(val, ctx);

            return this;
        }
    }
}
