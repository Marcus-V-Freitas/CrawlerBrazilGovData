using System;

namespace Application.Entities.DTOs
{
    public class DataSourceAditionalInformationDTO
    {
        public int Id { get;  set; }
        public string UrlFile { get;  set; }
        public DateTime? LastUpdate { get;  set; }
        public DateTime? CreationDate { get;  set; }
        public string Format { get;  set; }
        public string License { get;  set; }
        public string Created { get;  set; }
        public bool HasViews { get;  set; }
        public string InternalId { get;  set; }
        public string LastModified { get;  set; }
        public bool OnSameDomain { get;  set; }
        public string PackageId { get;  set; }
        public string RevisionId { get;  set; }
        public string State { get;  set; }
        public string UrlType { get;  set; }
    }
}