using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeKeeper.Domain.Utilities.Mappers
{
    public static class TimeEntryMapper
    {
        /// <summary>
        /// Returns a new DataAccess Entity with the values of the domain Model mapped to it.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static DataAccess.Entities.TimeEntry MapDomainToEntity(this Models.TimeEntry model)
        {
            DataAccess.Entities.TimeEntry entity = new()
            {
                CreatedBy       = model.CreatedBy,
                CreatedDate     = model.CreatedDate.ConvertLocalToUtc(),
                Id              = model.Id,
                UpdatedBy       = model.UpdatedBy,
                UpdatedDate     = model.UpdatedDate.ConvertLocalToUtc(),
                Accomplishment  = model.Accomplishment,
                EndWork         = model.EndWork?.ConvertLocalToUtc(),
                LaborerId       = model.LaborerId,
                StartWork       = model.StartWork?.ConvertLocalToUtc(),
                WorkItemId      = model.WorkItemId,
            };

            return entity;
        }

        /// <summary>
        /// Updates the values of an existing entity reference with the values from the Domain Model.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entity"></param>
        public static void MapDomainToEntity(this Models.TimeEntry model, ref DataAccess.Entities.TimeEntry entity)
        {
            entity.CreatedBy        = model.CreatedBy;
            entity.CreatedDate      = model.CreatedDate.ConvertLocalToUtc();
            entity.UpdatedBy        = model.UpdatedBy;
            entity.UpdatedDate      = model.UpdatedDate.ConvertLocalToUtc();
            entity.Accomplishment   = model.Accomplishment;
            entity.EndWork          = model.EndWork?.ConvertLocalToUtc();
            entity.LaborerId        = model.LaborerId;
            entity.StartWork        = model.StartWork?.ConvertLocalToUtc();
            entity.WorkItemId       = model.WorkItemId;
        }

        /// <summary>
        /// Returns a new Domain model with the values of the Entity mapped to it.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Models.TimeEntry MapEntityToDomain(this DataAccess.Entities.TimeEntry entity)
        {
            var model = new Models.TimeEntry()
            {
                CreatedBy       = entity.CreatedBy,
                CreatedDate     = entity.CreatedDate.ConvertUtcToLocal(),
                Id              = entity.Id,
                UpdatedBy       = entity.UpdatedBy,
                UpdatedDate     = entity.UpdatedDate.ConvertUtcToLocal(),
                EndWork         = entity.EndWork?.ConvertUtcToLocal(),
                LaborerId       = entity.LaborerId,
                StartWork       = entity.StartWork?.ConvertUtcToLocal(),
                WorkItemId      = entity.WorkItemId,
                Accomplishment = entity.Accomplishment,
            };

            if(entity.WorkItem != null)
            {
                model.WorkItem = entity.WorkItem.MapEntityToDomain();
            }

            return model;
        }
    }
}
