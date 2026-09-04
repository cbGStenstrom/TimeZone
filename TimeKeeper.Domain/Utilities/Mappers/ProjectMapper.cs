using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeKeeper.Domain.Utilities.Mappers
{
    public static class ProjectMapper
    {
        /// <summary>
        /// Returns a new DataAccess Entity with the values of the domain Model mapped to it.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static DataAccess.Entities.Project MapDomainToEntity(this Models.Project model)
        {
            DataAccess.Entities.Project entity = new()
            {
                CreatedBy   = model.CreatedBy,
                CreatedDate = model.CreatedDate.ConvertLocalToUtc(),
                Id          = model.Id,
                UpdatedBy   = model.UpdatedBy,
                UpdatedDate = model.UpdatedDate.ConvertLocalToUtc(),
                Key         = model.Key,
                LongName    = model.LongName,
                ShortName   = model.ShortName,             
            };

            return entity;
        }

        /// <summary>
        /// Updates the values of an existing entity reference with the values from the Domain Model.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entity"></param>
        public static void MapDomainToEntity(this Models.Project model, ref DataAccess.Entities.Project entity)
        {
            entity.CreatedBy    = model.CreatedBy;
            entity.CreatedDate  = model.CreatedDate.ConvertLocalToUtc();
            entity.UpdatedBy    = model.UpdatedBy;
            entity.UpdatedDate  = model.UpdatedDate.ConvertLocalToUtc();
            entity.Key          = model.Key;
            entity.LongName     = model.LongName;
            entity.ShortName    = model.ShortName;
        }

        /// <summary>
        /// Returns a new Domain model with the values of the Entity mapped to it.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Models.Project MapEntityToDomain(this DataAccess.Entities.Project entity)
        {
            var model = new Models.Project()
            {
                CreatedBy   = entity.CreatedBy,
                CreatedDate = entity.CreatedDate.ConvertUtcToLocal(),
                Id          = entity.Id,
                UpdatedBy   = entity.UpdatedBy,
                UpdatedDate = entity.UpdatedDate.ConvertUtcToLocal(),
                Key         = entity.Key,
                LongName    = entity.LongName,
                ShortName   = entity.ShortName,

            };

            return model;
        }



    }
}
