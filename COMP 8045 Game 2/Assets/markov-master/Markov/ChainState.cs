// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Markov
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a state in a Markov chain.
    /// </summary>
    /// <typeparam name="T">The type of the constituent parts of each state in the Markov chain.</typeparam>
    [Serializable()]
    public class ChainState<T> : IEquatable<ChainState<T>>, IList<T>
    {
        public readonly T[] items; //made from private to public for Unity testing

        /// <summary>
        /// Initializes a new instance of the <see cref="ChainState{T}"/> class with the specified items.
        /// </summary>
        /// <param name="items">An <see cref="IEnumerable{T}"/> of items to be copied as a single state.</param>
        public ChainState(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            this.items = items.ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChainState{T}"/> class with the specified items.
        /// </summary>
        /// <param name="items">An array of <typeparamref name="T"/> items to be copied as a single state.</param>
        public ChainState(params T[] items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            this.items = new T[items.Length];
            Array.Copy(items, this.items, items.Length);
        }

        /// <inheritdoc />
        public int Count => this.items.Length;

        /// <inheritdoc />
        public bool IsReadOnly => true;

        /// <inheritdoc />
        public T this[int index]
        {
            get { return this.items[index]; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="ChainState{T}"/> are not equal.
        /// </summary>
        /// <param name="a">The first <see cref="ChainState{T}"/> to compare.</param>
        /// <param name="b">The second <see cref="ChainState{T}"/> to compare.</param>
        /// <returns>true if <paramref name="a"/> and <paramref name="b"/> do not represent the same state; otherwise, false.</returns>
        public static bool operator !=(ChainState<T> a, ChainState<T> b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="ChainState{T}"/> are equal.
        /// </summary>
        /// <param name="a">The first <see cref="ChainState{T}"/> to compare.</param>
        /// <param name="b">The second <see cref="ChainState{T}"/> to compare.</param>
        /// <returns>true if <paramref name="a"/> and <paramref name="b"/> represent the same state; otherwise, false.</returns>
        public static bool operator ==(ChainState<T> a, ChainState<T> b)
        {
            if (object.ReferenceEquals(a, b))
            {
                return true;
            }
            else if (object.Equals(null, a) || object.Equals(null, b)) //edited from "a is null" and "b is null"
            {
                return false;
            }

            return a.Equals(b);
        }

        /// <inheritdoc />
        public void Add(T item) {
			throw new NotSupportedException();
		}

        /// <inheritdoc />
        public void Clear() { 
			throw new NotSupportedException();
		}

        /// <inheritdoc />
        public bool Contains(T item) => ((IList<T>)this.items).Contains(item);

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex) => this.items.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is ChainState<T>)
            {
				var chain = (ChainState<T>)obj;
                return this.Equals(chain);
            }

            return false;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
        public bool Equals(ChainState<T> other)
        {
            if (object.Equals(null, other)) //J: edited from "other is null" ...noting of whether such would work ...
            {
                return false;
            }

            if (this.items.Length != other.items.Length)
            {
                return false;
            }

            for (var i = 0; i < this.items.Length; i++)
            {
                if (!this.items[i].Equals(other.items[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() => ((IList<T>)this.items).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var code = this.items.Length;

            for (var i = 0; i < this.items.Length; i++)
            {
                code = (code * 37) + this.items[i].GetHashCode();
            }

            return code;
        }

        /// <inheritdoc />
        public int IndexOf(T item) {
			throw new NotSupportedException();
		}

        /// <inheritdoc />
        public void Insert(int index, T item) {
			throw new NotSupportedException();
		}

        /// <inheritdoc />
        public bool Remove(T item) {
			throw new NotSupportedException();
		}

        /// <inheritdoc />
        public void RemoveAt(int index) {
			throw new NotSupportedException();
		}
    }
}
