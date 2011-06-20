using System;

namespace ProductionProfiler.Core.Persistence.Entities
{
    /// <summary>
    /// Information about Paginated results.
    /// </summary>
    [Serializable]
    public class Pagination
    {
        [NonSerialized]
        private readonly Func<int> _totalRecords;

        public Pagination(int pageSize, int pageNumber, int totalRecords)
            : this(pageSize, pageNumber, () => totalRecords)
        {}

        public Pagination(int pageSize, int pageNumber, Func<int> totalRecords)
            : this(pageSize, pageNumber)
        {
            _totalRecords = totalRecords;
            Calculate();
        }

        private Pagination(int pageSize, int pageNumber)
        {
            PageSize = pageSize == 0 ? int.MaxValue : pageSize;
            PageNumber = pageNumber == 0 ? 1 : pageNumber;
        }

        private void Calculate()
        {
            if (_calculated)
                return;

            _calculated = true;

            var totalRecords = _totalRecords();

            int pageCount = totalRecords / PageSize + (totalRecords % PageSize == 0 ? 0 : 1);

            _firstItem = ((PageNumber - 1) * PageSize) + 1;
            _hasNextPage = PageNumber < pageCount;
            _hasPreviousPage = PageNumber > 1;
            _lastItem = PageNumber < pageCount ? ((PageNumber - 1) * PageSize) + PageSize : totalRecords;
            _totalItems = totalRecords;
            _totalPages = pageCount;
        }

        private bool _calculated = false;
        private int _firstItem;
        private bool _hasNextPage;
        private bool _hasPreviousPage;
        private int _lastItem;
        private int _totalItems;
        private int _totalPages;

        public int PageNumber { get; private set; }

        public int PageSize { get; private set; }

        public int FirstItem
        {
            get
            {
                Calculate();
                return _firstItem;
            }
        }

        public bool HasNextPage
        {
            get
            {
                Calculate();
                return _hasNextPage;
            }
        }

        public bool HasPreviousPage
        {
            get
            {
                Calculate();
                return _hasPreviousPage;
            }
        }

        public int LastItem
        {
            get
            {
                Calculate();
                return _lastItem;
            }
        }

        public int TotalItems
        {
            get
            {
                Calculate();
                return _totalItems;
            }
        }

        public int TotalPages
        {
            get
            {
                Calculate();
                return _totalPages;
            }
        }
    }
}
