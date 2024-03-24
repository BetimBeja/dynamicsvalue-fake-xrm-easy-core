﻿#if FAKE_XRM_EASY_9
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.FakeMessageExecutors;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FakeXrmEasy.Middleware.Crud.FakeMessageExecutors
{
    /// <summary>
    /// UpdateMultipleRequest Executor
    /// </summary>
    public class UpdateMultipleRequestExecutor : IFakeMessageExecutor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool CanExecute(OrganizationRequest request)
        {
            return request is UpdateMultipleRequest;
        }

        /// <summary>
        /// Executes the CreateRequestMultiple request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public OrganizationResponse Execute(OrganizationRequest request, IXrmFakedContext ctx)
        {
            var updateMultipleRequest = (UpdateMultipleRequest)request;

            ValidateRequest(updateMultipleRequest, ctx);
            
            var records = updateMultipleRequest.Targets.Entities;

            var service = ctx.GetOrganizationService();
            
            foreach (var record in records)
            {
                service.Update(record);
            }

            return new UpdateMultipleResponse()
            {
                ResponseName = "UpdateMultipleResponse"
            };
        }

        private void ValidateRequiredParameters(UpdateMultipleRequest request)
        {
            if (request.Targets == null)
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.InvalidArgument,
                    "Required field 'Targets' is missing");
            }

            var targets = request.Targets;
            if (string.IsNullOrWhiteSpace(targets.EntityName))
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.InvalidArgument,
                    "Required member 'EntityName' missing for field 'Targets'");
            }
        }

        private void ValidateRecords(UpdateMultipleRequest request, IXrmFakedContext ctx)
        {
            var records = request.Targets.Entities;
            if (records.Count == 0)
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.UnExpected,
                    $"System.ArgumentException: The value of the parameter 'Targets' cannot be null or empty.");
            }

            foreach (var record in records)
            {
                ValidateRecord(request, record, ctx);
            }
        }

        private void ValidateRecord(UpdateMultipleRequest request, Entity recordToUpdate, IXrmFakedContext ctx)
        {
            if (!request.Targets.EntityName.Equals(recordToUpdate.LogicalName))
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.QueryBuilderNoAttribute,
                    $"This entity cannot be added to the specified collection. The collection can have entities with PlatformName = {request.Targets.EntityName} while this entity has Platform Name: {recordToUpdate.LogicalName}");
            }

            if (recordToUpdate.Id == Guid.Empty)
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.ObjectDoesNotExist,
                    $"Entity Id must be specified for Operation");
            }
            
            var exists = ctx.ContainsEntity(recordToUpdate.LogicalName, recordToUpdate.Id);
            if (!exists)
            {
                throw FakeOrganizationServiceFaultFactory.New(ErrorCodes.ObjectDoesNotExist,
                    $"{recordToUpdate.LogicalName} With Ids = {recordToUpdate.Id} Do Not Exist");
            }
        }

        private void ValidateRequest(UpdateMultipleRequest request, IXrmFakedContext ctx)
        {
            ValidateRequiredParameters(request);
            BulkOperationsCommon.ValidateEntityName(request.Targets.EntityName, ctx);
            ValidateRecords(request, ctx);
        }

        /// <summary>
        /// Returns CreateMultipleRequest
        /// </summary>
        /// <returns></returns>
        public Type GetResponsibleRequestType()
        {
            return typeof(UpdateMultipleRequest);
        }
    }
}
#endif