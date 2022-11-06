using System.Collections.Generic;

namespace Polyperfect.Crafting.Framework
{
    /// <summary>
    ///     Something that allows access to a dictionary of a particular type.
    /// </summary>
    /// <typeparam name="KEY">The type used as a key for the dictionary.</typeparam>
    public interface IReadOnlyDataAccessorLookup<KEY>
    {
        /// <summary>
        ///     Get a dictionary allowing reading values based off the KEY type.
        /// </summary>
        /// <typeparam name="VALUE">The data type returned.</typeparam>
        IReadOnlyDictionary<KEY, VALUE> GetReadOnlyAccessor<VALUE>();
    }

    /// <summary>
    ///     Something that allows access to a dictionary of a particular type.
    /// </summary>
    /// <typeparam name="KEY">The type used as a key for the dictionary.</typeparam>
    public interface IDataAccessorLookup<KEY> : IReadOnlyDataAccessorLookup<KEY>
    {
        /// <summary>
        ///     Get a dictionary allowing reading and writing values based off the KEY type.
        /// </summary>
        /// <typeparam name="VALUE">The data type returned.</typeparam>
        IDictionary<KEY, VALUE> GetAccessor<VALUE>();
    }

    /// <summary>
    ///     Something that allows access to a dictionary of a particular type.
    /// </summary>
    /// <typeparam name="KEY">The type used as a key for the dictionary.</typeparam>
    public interface IReadOnlyDataAccessorLookupWithArg<KEY, ARG>
    {
        /// <summary>
        ///     Get a dictionary allowing reading values based off the KEY type. An argument is used to specify which dictionary to
        ///     use.
        /// </summary>
        /// <typeparam name="VALUE">The data type returned.</typeparam>
        IReadOnlyDictionary<KEY, VALUE> GetReadOnlyAccessor<VALUE>(ARG arg);
    }

    /// <summary>
    ///     Something that allows access to a dictionary of a particular type.
    /// </summary>
    /// <typeparam name="KEY">The type used as a key for the dictionary.</typeparam>
    /// <typeparam name="ARG">The type passed as an argument to specify which accessor to use.</typeparam>
    public interface IDataAccessorLookupWithArg<KEY, ARG> : IReadOnlyDataAccessorLookupWithArg<KEY, ARG>
    {
        /// <summary>
        ///     Get a dictionary allowing reading values based off the KEY type. An argument is used to specify which dictionary to
        ///     use.
        /// </summary>
        /// <typeparam name="VALUE">The data type returned.</typeparam>
        IDictionary<KEY, VALUE> GetAccessor<VALUE>(ARG arg);
    }
}