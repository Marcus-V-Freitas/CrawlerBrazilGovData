using System;

namespace Domain.Entities
{
    public class DataSourceAditionalInformation
    {
        public int Id { get; private set; }
        public DateTime? LastUpdate { get; private set; }
        public DateTime? CreationDate { get; private set; }
        public string Format { get; private set; }
        public string License { get; private set; }
        public string Created { get; private set; }
        public bool HasViews { get; private set; }
        public string InternalId { get; private set; }
        public string LastModified { get; private set; }
        public bool OnSameDomain { get; private set; }
        public string PackageId { get; private set; }
        public string RevisionId { get; private set; }
        public string State { get; private set; }
        public string UrlType { get; private set; }

        public DataSourceAditionalInformation()
        {
        }

        public DataSourceAditionalInformation(DateTime? lastUpdate, DateTime? creationDate, string format,
                                              string license, string created, bool hasViews,
                                              string internalId, string lastModified, bool onSameDomain,
                                              string packageId, string revisionId, string state, string urlType)
        {
            LastUpdate = lastUpdate;
            CreationDate = creationDate;
            Format = format;
            License = license;
            Created = created;
            HasViews = hasViews;
            InternalId = internalId;
            LastModified = lastModified;
            OnSameDomain = onSameDomain;
            PackageId = packageId;
            RevisionId = revisionId;
            State = state;
            UrlType = urlType;
        }
    }
}