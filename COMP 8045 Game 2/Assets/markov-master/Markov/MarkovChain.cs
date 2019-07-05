// Copyright © John Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace Markov
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Builds and walks interconnected states based on a weighted probability.
    /// </summary>
    /// <typeparam name="T">The type of the constituent parts of each state in the Markov chain.</typeparam>
    [Serializable()]
    public class MarkovChain<T>
        where T : IEquatable<T>
    {
        public readonly Dictionary<ChainState<T>, Dictionary<T, int>> items = new Dictionary<ChainState<T>, Dictionary<T, int>>(); //made from private to public for Unity testing
        private readonly int order;
        private readonly Dictionary<ChainState<T>, int> terminals = new Dictionary<ChainState<T>, int>(); //J: terminal ...

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkovChain{T}"/> class.
        /// </summary>
        /// <param name="order">Indicates the desired order of the <see cref="MarkovChain{T}"/>.</param>
        /// <remarks>
        /// <para>The <paramref name="order"/> of a generator indicates the depth of its internal state.  A generator
        /// with an order of 1 will choose items based on the previous item, a generator with an order of 2
        /// will choose items based on the previous 2 items, and so on.</para>
        /// <para>Zero is not classically a valid order value, but it is allowed here.  Choosing a zero value has the
        /// effect that every state is equivalent to the starting state, and so items will be chosen based on their
        /// total frequency.</para>
        /// </remarks>
        public MarkovChain(int order)
        {
            if (order < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(order));
            }

            this.order = order;
        }

        /// <summary>
        /// Adds the items to the generator with a weight of one.
        /// </summary>
        /// <param name="items">The items to add to the generator.</param>
        public void Add(IEnumerable<T> items) => this.Add(items, 1);

        /// <summary>
        /// Adds the items to the generator with the weight specified.
        /// </summary>
        /// <param name="items">The items to add to the generator.</param>
        /// <param name="weight">The weight at which to add the items.</param>
        public void Add(IEnumerable<T> items, int weight)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var previous = new Queue<T>();

            //add all of the items to previous that can be held by a ChainState, with order as the max amount, and
            //updating weights with respect to the previous ChainState on each addition
            foreach (var item in items)
            {
                var key = new ChainState<T>(previous); //assign <<current state><YKWIM>> to key

                this.Add(key, item, weight); //add item with the respective weight to <<current state><YKWIM>>'s <<items><YKWIM>> and respective distribution

                //update queue <<- aka current state -><YKWIM>> with item
                previous.Enqueue(item);
                if (previous.Count > this.order)
                {
                    previous.Dequeue();
                }
            }

            //J: Adding a terminal ChainState to terminals - being the final state of the items - though not to items
            var terminalKey = new ChainState<T>(previous);
            var newWeight = Math.Max(0, this.terminals.ContainsKey(terminalKey)
                ? weight + this.terminals[terminalKey]
                : weight);
            if (newWeight == 0)
            {
                this.terminals.Remove(terminalKey); //removed from where it could've been less than 0?
            }
            else
            {
                this.terminals[terminalKey] = newWeight;
            }
        }

        /// <summary>
        /// Adds the item to the generator, with the specified items preceding it.
        /// </summary>
        /// <param name="previous">The items preceding the item.</param>
        /// <param name="item">The item to add.</param>
        /// <remarks>
        /// See <see cref="MarkovChain{T}.Add(IEnumerable{T}, T, int)"/> for remarks.
        /// </remarks>
        public void Add(IEnumerable<T> previous, T item)
        {
            if (previous == null)
            {
                throw new ArgumentNullException(nameof(previous));
            }

            var state = new Queue<T>(previous);
            while (state.Count > this.order)
            {
                state.Dequeue();
            }

            this.Add(new ChainState<T>(state), item, 1);
        }

        /// <summary>
        /// Adds the item to the generator, with the specified state preceding it.
        /// </summary>
        /// <param name="state">The state preceding the item.</param>
        /// <param name="next">The item to add.</param>
        /// <remarks>
        /// See <see cref="MarkovChain{T}.Add(ChainState{T}, T, int)"/> for remarks.
        /// </remarks>
        public void Add(ChainState<T> state, T next) => this.Add(state, next, 1);

        /// <summary>
        /// Adds the item to the generator, with the specified items preceding it and the specified weight.
        /// </summary>
        /// <param name="previous">The items preceding the item.</param>
        /// <param name="item">The item to add.</param>
        /// <param name="weight">The weight of the item to add.</param>
        /// <remarks>
        /// This method does not add all of the preceding states to the generator.
        /// Notably, the empty state is not added, unless the <paramref name="previous"/> parameter is empty.
        /// </remarks>
        public void Add(IEnumerable<T> previous, T item, int weight)
        {
            if (previous == null)
            {
                throw new ArgumentNullException(nameof(previous));
            }

            var state = new Queue<T>(previous);
            while (state.Count > this.order)
            {
                state.Dequeue();
            }

            this.Add(new ChainState<T>(state), item, weight);
        }

        /// <summary>
        /// Adds the item to the generator, with the specified state preceding it and the specified weight.
        /// </summary>
        /// <param name="state">The state preceding the item.</param>
        /// <param name="next">The item to add.</param>
        /// <param name="weight">The weight of the item to add.</param>
        /// <remarks>
        /// This adds the state as-is.  The state may not be reachable if, for example, the
        /// number of items in the state is greater than the order of the generator, or if the
        /// combination of items is not available in the other states of the generator.
        ///
        /// A negative weight may be passed, which will have the impact of reducing the weight
        /// of the specified state transition.  This can therefore be used to remove items from
        /// the generator. The resulting weight will never be allowed below zero.
        /// </remarks>
        public void Add(ChainState<T> state, T next, int weight)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

			Dictionary<T, int> weights;
            if (!this.items.TryGetValue(state, out weights))
            {
                weights = new Dictionary<T, int>();
                this.items.Add(state, weights);
            }

            var newWeight = Math.Max(0, weights.ContainsKey(next)
                ? weight + weights[next]
                : weight);
            if (newWeight == 0)
            {
                weights.Remove(next);
                if (weights.Count == 0)
                {
                    this.items.Remove(state);
                }
            }
            else
            {
                weights[next] = newWeight;
            }
        }

        /// <summary>
        /// Randomly walks the chain.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of the items chosen.</returns>
        /// <remarks>Assumes an empty starting state.</remarks>
        public IEnumerable<T> Chain() => this.Chain(Enumerable.Empty<T>(), new Random());

        /// <summary>
        /// Randomly walks the chain.
        /// </summary>
        /// <param name="previous">The items preceding the first item in the chain.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of the items chosen.</returns>
        public IEnumerable<T> Chain(IEnumerable<T> previous) => this.Chain(previous, new Random());

        /// <summary>
        /// Randomly walks the chain.
        /// </summary>
        /// <param name="seed">The seed for the random number generator, used as the random number source for the chain.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of the items chosen.</returns>
        /// <remarks>Assumes an empty starting state.</remarks>
        public IEnumerable<T> Chain(int seed) => this.Chain(Enumerable.Empty<T>(), new Random(seed));

        /// <summary>
        /// Randomly walks the chain.
        /// </summary>
        /// <param name="previous">The items preceding the first item in the chain.</param>
        /// <param name="seed">The seed for the random number generator, used as the random number source for the chain.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of the items chosen.</returns>
        public IEnumerable<T> Chain(IEnumerable<T> previous, int seed) => this.Chain(previous, new Random(seed));

        /// <summary>
        /// Randomly walks the chain.
        /// </summary>
        /// <param name="rand">The random number source for the chain.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of the items chosen.</returns>
        /// <remarks>Assumes an empty starting state.</remarks>
        public IEnumerable<T> Chain(Random rand) => this.Chain(Enumerable.Empty<T>(), rand);

        /// <summary>
        /// Randomly walks the chain.
        /// </summary>
        /// <param name="previous">The items preceding the first item in the chain.</param>
        /// <param name="rand">The random number source for the chain.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of the items chosen.</returns>
        //J: getting the next item and\or state in the chain ... via selecting weights ... would be done by ...
        public IEnumerable<T> Chain(IEnumerable<T> previous, Random rand)
        {
            if (previous == null)
            {
                throw new ArgumentNullException(nameof(previous));
            }
            else if (rand == null)
            {
                throw new ArgumentNullException(nameof(rand));
            }

            var state = new Queue<T>(previous);
            while (true)
            {
                //J: <<in each iteration, ><YKWIM>>select an item at random across the <weighted distribution> and Enqueue such to the state queue

                while (state.Count > this.order)
                {
                    state.Dequeue(); //J: Reduce size to the order ...
                }

                var key = new ChainState<T>(state);

				Dictionary<T, int> weights; //TValue of this.items
                if (!this.items.TryGetValue(key, out weights)) //J: break out of...an outer foreach loop that would be iterating the current iterator<< and\or such><YKWIM>>? Where <<if an item would not ><YKWIM>>be gettable, then break out?
                {
                    yield break;
                }

				int terminalWeight; //TValue of this.terminals - the terminal weight value of the current state?
                this.terminals.TryGetValue(key, out terminalWeight);

                var total = weights.Sum(w => w.Value); //J: total weight of all of the items across all states? Or the current state ...
                var value = rand.Next(total + terminalWeight) + 1; //J: random value across the items + terminal up to the total weight?

                if (value > total)
                {
                    yield break; //break out of foreach because ...went above the <<terminal weight><YKWIM>>?
                }

                var currentWeight = 0;
                foreach (var nextItem in weights) //iterate through the weights and select the first <<item><YKWIM>> that would <<reach the ><YKWIM>><<random value threshold><YKWIM>> to add to the state
                {
                    currentWeight += nextItem.Value;
                    if (currentWeight >= value)
                    {
                        yield return nextItem.Key;
                        state.Enqueue(nextItem.Key);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the items from the generator that follow from an empty state.
        /// </summary>
        /// <returns>A dictionary of the items and their weight.</returns>
        public Dictionary<T, int> GetInitialStates() => this.GetNextStates(new ChainState<T>(Enumerable.Empty<T>()));

        /// <summary>
        /// Gets the items from the generator that follow from the specified items preceding it.
        /// </summary>
        /// <param name="previous">The items preceding the items of interest.</param>
        /// <returns>A dictionary of the items and their weight.</returns>
        public Dictionary<T, int> GetNextStates(IEnumerable<T> previous)
        {
            var state = new Queue<T>(previous);
            while (state.Count > this.order)
            {
                state.Dequeue();
            }

            return this.GetNextStates(new ChainState<T>(state));
        }

        /// <summary>
        /// Gets the items from the generator that follow from the specified state preceding it.
        /// </summary>
        /// <param name="state">The state preceding the items of interest.</param>
        /// <returns>A dictionary of the items and their weight.</returns>
        public Dictionary<T, int> GetNextStates(ChainState<T> state)
        {
			Dictionary<T, int> weights;
            if (this.items.TryGetValue(state, out weights))
            {
                return new Dictionary<T, int>(weights);
            }

            return null;
        }

        /// <summary>
        /// Returns an IEnumerable that would have the next item queued, given the previous items.
        /// </summary>
        /// <param name="previous"></param>
        /// <param name="rand"></param>
        /// <returns></returns>
        public IEnumerable<T> NextItem (IEnumerable<T> previous, Random rand)
        {
            var state = new Queue<T>(previous);
            while (state.Count > this.order)
            {
                state.Dequeue(); //J: Reduce size to the order ... with <<making it into a state with such number of items with such and the order><YKWIM>>
            }

            var key = new ChainState<T>(state);

            Dictionary<T, int> weights; //TValue of this.items
            if (!this.items.TryGetValue(key, out weights)) //J: break out of...an outer foreach loop that would be iterating the current iterator<< and\or such><YKWIM>>? Where <<if an item would not ><YKWIM>>be gettable, then break out?
                //noting of comparing by reference ... of ChainStates ... and whether such would be done in TryGetValue
                //and whether such a ChainState would exist ... well, being the prev. and\or curr. of AIData ...
                //also, noting of whether such of ChainStates would be new or what would be existing references, and such in TryGetValue, with such in transitions and\or such and any resulting IEnumerables and\or such and ChainStates regarding such IEnumerables that would be made into ChainStates ...
            {
                UnityEngine.Debug.Log("break here1");
                yield break;
                //return null;
            }

            //exclude terminations in transitions as they would not be intended to be reflected as occurring - so, editing the following regarding such in getting the next item; the end of an <<IEnumerable><YKWIM>> would be intended as just the end of the chain where the end would not have any sort of transition<< such as termination><YKWIM>> to be added as data

            int terminalWeight; //TValue of this.terminals - the terminal weight value of the current state?
            //UnityEngine.Debug.Log("this.terminals.TryGetValue(key, out terminalWeight): "+this.terminals.TryGetValue(key, out terminalWeight));

            var total = weights.Sum(w => w.Value); //J: total weight of all of the items transitioning from the current state ...
            var value = rand.Next(total/* + terminalWeight*/) + 1; //J: random value across the items + terminal up to the total weight?
            //UnityEngine.Debug.Log("total: " + total + "; terminalWeight: " + terminalWeight);
            if (value > total)
            {
                UnityEngine.Debug.Log("break here2");
                yield break; //break out of foreach because ...went above the <<terminal weight><YKWIM>>? Well, I guess this would be meaning that a terminal state <<would have been selected><YKWIM>> among<< the ><YKWIM>>distribution and\or termination would have been selected? With such and being as such could occur in training? With some <<frequency\rate><YKWIM>> in training and\or such ...
                             //though a terminal state would be a new state, and would just be one that would not have transitions - IOW, it would have broken out of "break here1"<< above><YKWIM>> if this would be the terminal state?
                             //  -And if it would mean a transition to a terminal state, then such transitioning to such still ...
                             //...well, this being the weight of the state in the "terminals" section of this state, in particular ...with such a terminalWeight being a weight across all items<< and\or such><YKWIM>> transitioning to this state when this state <<would have been the terminal state on the addition of the sequence including this terminal state><YKWIM>>
                             //maybe intended as termination with eg. words, with such a probability of termination as <<equal><YKWIM>> to the frequency of termination in the training data
                //among < the weight leading up to the state when it would have been terminal and the weights transitioning away from the state when it wouldn't have been terminal?> Actually, such being among <the weight of the state's 'termination' << 'transition' >< YKWIM >> and the weights transitioning away from the state when it wouldn't have been terminal?>
                //        or < the weight of the state's 'lack of transition' alongside the weights transitioning away ...>
                    //and noting of how such would not be how the Markov chain would be used in this game - with there not being a termination of behaviour transitions - so can just remove this part regarding the chain
            }

            var currentWeight = 0;
            foreach (var nextItem in weights) //iterate through the weights and select the first <<item><YKWIM>> that would <<reach the ><YKWIM>><<random value threshold><YKWIM>> to add to the state
            {
                currentWeight += nextItem.Value;
                if (currentWeight >= value)
                {
                    yield return nextItem.Key;
                    state.Enqueue(nextItem.Key);
                    state.Dequeue(); //remove one item alongside the additional item
                    //UnityEngine.Debug.Log("break here3");
                    break;
                }
            }
        }

        /// <summary>
        /// Gets all of the states that exist in the generator.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="ChainState{T}"/> containing all of the states in the generator.</returns>
        public IEnumerable<ChainState<T>> GetStates()
        {
            foreach (var state in this.items.Keys)
            {
                yield return state;
            }

            foreach (var state in this.terminals.Keys)
            {
                if (!this.items.ContainsKey(state))
                {
                    yield return state;
                }
            }
        }

        /// <summary>
        /// Gets the weight of termination following from the specified items.
        /// </summary>
        /// <param name="previous">The items preceding the items of interest.</param>
        /// <returns>A dictionary of the items and their weight.</returns>
        public int GetTerminalWeight(IEnumerable<T> previous)
        {
            var state = new Queue<T>(previous);
            while (state.Count > this.order)
            {
                state.Dequeue();
            }

            return this.GetTerminalWeight(new ChainState<T>(state));
        }

        /// <summary>
        /// Gets the weights of termination following from the specified state.
        /// </summary>
        /// <param name="state">The state preceding the items of interest.</param>
        /// <returns>A dictionary of the items and their weight.</returns>
        public int GetTerminalWeight(ChainState<T> state)
        {
			int weight;
            this.terminals.TryGetValue(state, out weight);
            return weight;
        }
    }
}
