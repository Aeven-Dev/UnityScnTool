using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace NetsphereScnTool.Utility
{
    public class SortableBindingList<T> : BindingList<T> where T : class
    {
        private List<int> _linkedIDs;

        public List<int> LinkedIDs
        {
            get => _linkedIDs;
            set => _linkedIDs = value;
        }

        private bool _isModified;

        public bool IsModified
        {
            get => _isModified;
            set => _isModified = value;
        }

        private bool _isSorted;
        private ListSortDirection _sortDirection = ListSortDirection.Ascending;
        private PropertyDescriptor _sortProperty;

        public SortableBindingList()
        {
        }

        public SortableBindingList(IList<T> list)
          : base(list)
        {
        }

        public SortableBindingList(IEnumerable<T> data)
        {
            if (data != null)
            {
                foreach (var item in data)
                    Add(item);
            }
        }

        public void AddRange(IEnumerable<T> vals)
        {
            var collection = vals as ICollection<T>;
            if (collection != null)
            {
                int requiredCapacity = Count + collection.Count;
                //if (requiredCapacity > _linkedIDs.Capacity)
                //  _linkedIDs.Capacity = requiredCapacity;
            }

            bool restore = RaiseListChangedEvents;
            try
            {
                RaiseListChangedEvents = false;
                foreach (var v in vals)
                    Add(v); // We cant call _baseList.Add, otherwise Events wont get hooked.
            }
            finally
            {
                RaiseListChangedEvents = restore;
                if (RaiseListChangedEvents)
                    ResetBindings();
            }
        }

        protected override bool SupportsSortingCore => true;

        protected override bool IsSortedCore => _isSorted;

        protected override ListSortDirection SortDirectionCore => _sortDirection;

        protected override PropertyDescriptor SortPropertyCore => _sortProperty;

        protected override void RemoveSortCore()
        {
            _sortDirection = ListSortDirection.Ascending;
            _sortProperty = null;
            _isSorted = false; //thanks Luca
        }

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            _sortProperty = prop;
            _sortDirection = direction;

            Sort(Compare);
        }

        public void Sort(PropertyDescriptor prop, ListSortDirection direction) => ApplySortCore(prop, direction);

        public void Sort(Comparison<T> comparison)
        {
            var list = Items as List<T>;
            if (list == null)
                return;

            list.Sort(comparison);

            _isSorted = true;
            //fire an event that the list has been changed.
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        private int Compare(T lhs, T rhs)
        {
            int result = OnComparison(lhs, rhs);
            //invert if descending
            if (_sortDirection == ListSortDirection.Descending)
                result = -result;
            return result;
        }

        private int OnComparison(T lhs, T rhs)
        {
            object lhsValue = lhs == null ? null : _sortProperty.GetValue(lhs);
            object rhsValue = rhs == null ? null : _sortProperty.GetValue(rhs);
            if (lhsValue == null)
            {
                return (rhsValue == null) ? 0 : -1; //nulls are equal
            }
            if (rhsValue == null)
            {
                return 1; //first has value, second doesn't
            }
            if (lhsValue is IComparable)
            {
                return ((IComparable)lhsValue).CompareTo(rhsValue);
            }
            if (lhsValue.Equals(rhsValue))
            {
                return 0; //both are the same
            }
            //not comparable, compare ToString
            return lhsValue.ToString().CompareTo(rhsValue.ToString());
        }

        public IList<T> InnerItems => Items;

    }
}
