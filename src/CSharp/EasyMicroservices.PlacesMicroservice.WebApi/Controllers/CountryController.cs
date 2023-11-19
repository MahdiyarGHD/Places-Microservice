﻿using EasyMicroservices.ContentsMicroservice.Clients.Helpers;
using EasyMicroservices.Cores.AspCoreApi;
using EasyMicroservices.Cores.AspEntityFrameworkCoreApi.Interfaces;
using EasyMicroservices.Cores.Contracts.Requests;
using EasyMicroservices.PlacesMicroservice.Contracts.Common;
using EasyMicroservices.PlacesMicroservice.Contracts.Requests;
using EasyMicroservices.PlacesMicroservice.Database.Entities;
using EasyMicroservices.ServiceContracts;
using Microsoft.AspNetCore.Mvc;
using GetByLanguageRequestContract = Contents.GeneratedServices.GetByLanguageRequestContract;

namespace EasyMicroservices.PlacesMicroservice.WebApi.Controllers
{
    public class CountryController : SimpleQueryServiceController<CountryEntity, CreateCountryRequestContract, UpdateCountryRequestContract, CountryContract, long>
    {
        private readonly IConfiguration _config;
        private readonly ContentLanguageHelper _contentHelper;
        readonly IUnitOfWork _unitOfWork;
        public CountryController(IUnitOfWork unitOfWork, IConfiguration config) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _config = config;
            _contentHelper = new(new Contents.GeneratedServices.ContentClient(_config.GetValue<string>("RootAddresses:Content"), new HttpClient()));
        }

        public override async Task<MessageContract<long>> Add(CreateCountryRequestContract request, CancellationToken cancellationToken = default)
        {
            var result = await base.Add(request, cancellationToken);
            if (result)
            {
                var addedItem = await GetById(new Cores.Contracts.Requests.GetIdRequestContract<long> { Id = result.Result })
                    .AsCheckedResult();
                request.UniqueIdentity = addedItem.UniqueIdentity;
                await _contentHelper.AddToContentLanguage(request)
                    .AsCheckedResult();
            }
            return result;
        }
        public override async Task<ListMessageContract<CountryContract>> Filter(FilterRequestContract filterRequest, CancellationToken cancellationToken = default)
        {
            var result = await base.Filter(filterRequest, cancellationToken);
            result.ThrowsIfFails();
            await _contentHelper.ResolveContentLanguage(result.Result, filterRequest.Language);
            return result;
        }


        public override async Task<MessageContract<CountryContract>> Update(UpdateCountryRequestContract request, CancellationToken cancellationToken = default)
        {
            var result = await base.Update(request, cancellationToken).AsCheckedResult();
            request.UniqueIdentity = result.UniqueIdentity;
            await _contentHelper.UpdateToContentLanguage(request).AsCheckedResult();
            return result;
        }

        public override async Task<MessageContract<CountryContract>> UpdateChangedValuesOnly(UpdateCountryRequestContract request, CancellationToken cancellationToken = default)
        {
            var result = await base.UpdateChangedValuesOnly(request, cancellationToken).AsCheckedResult();
            request.UniqueIdentity = result.UniqueIdentity;
            await _contentHelper.UpdateToContentLanguage(request).AsCheckedResult();
            return result;
        }

        [HttpPost]
        public async Task<ListMessageContract<CountryContract>> GetAllByLanguage(GetByLanguageRequestContract request, CancellationToken cancellationToken = default)
        {
            var result = await base.GetAll(cancellationToken);
            if (result)
            {
                await _contentHelper.ResolveContentLanguage(result.Result, request.Language);
            }
            return result;
        }

        [HttpPost]
        public async Task<MessageContract<CountryLanguageContract>> GetByIdAllLanguage(GetIdRequestContract<long> request, CancellationToken cancellationToken = default)
        {
            var result = await base.GetById(request.Id, cancellationToken);
            if (result)
            {
                var mapped = _unitOfWork.GetMapper().Map<CountryLanguageContract>(result.Result);
                await _contentHelper.ResolveContentAllLanguage(mapped);
                return mapped;
            }
            return result.ToContract<CountryLanguageContract>();
        }
    }
}
