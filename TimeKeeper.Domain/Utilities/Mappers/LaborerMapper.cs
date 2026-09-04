using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace TimeKeeper.Domain.Utilities.Mappers
{
    public static class LaborerMapperExtensions
    {
        /// <summary>
        /// Returns a new DataAccess Entity with the values of the domain Model mapped to it.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static DataAccess.Entities.Laborer MapDomainToEntity(this Models.Laborer model)
        {
            DataAccess.Entities.Laborer entity = new()
            {
                CreatedBy       = model.CreatedBy,
                CreatedDate     = model.CreatedDate.ConvertLocalToUtc(),
                Email           = model.Email,
                FirstName       = model.FirstName,
                MiddleName      = model.MiddleName,
                LastName        = model.LastName,
                Id              = model.Id,
                UpdatedBy       = model.UpdatedBy,
                UpdatedDate     = model.UpdatedDate.ConvertLocalToUtc(),
                Username        = model.Username,
            };

            return entity;
        }

        /// <summary>
        /// Updates the values of an existing entity reference with the values from the Domain Model.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entity"></param>
        public static void MapDomainToEntity(this Models.Laborer model, ref DataAccess.Entities.Laborer entity)
        {
            entity.CreatedBy = model.CreatedBy;
            entity.CreatedDate = model.CreatedDate.ConvertLocalToUtc();
            entity.Email = model.Email;
            entity.FirstName = model.FirstName;
            entity.MiddleName = model.MiddleName;
            entity.LastName = model.LastName;
            entity.UpdatedBy = model.UpdatedBy;
            entity.UpdatedDate = model.UpdatedDate.ConvertLocalToUtc();
            entity.Username = model.Username;
        }

        /// <summary>
        /// Returns a new Domain model with the values of the Entity mapped to it.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Models.Laborer MapEntityToDomain(this DataAccess.Entities.Laborer entity)
        {
            var model = new Models.Laborer()
            {
                CreatedBy       = entity.CreatedBy,
                CreatedDate     = entity.CreatedDate.ConvertUtcToLocal(),
                Email           = entity.Email,
                FirstName       = entity.FirstName,
                MiddleName      = entity.MiddleName,
                LastName        = entity.LastName,
                Id              = entity.Id,
                UpdatedBy       = entity.UpdatedBy,
                UpdatedDate     = entity.UpdatedDate.ConvertUtcToLocal(),
                Username        = entity.Username,
            };

            //if (entity.TimeEntries != null)
            //{
            //    foreach (var entity in entity.TimeEntries)
            //    {
            //        model.TimeEntries.Add(entity.MapFromEntity(entry));
            //    }
            //}

            return model;
        }

    }
}
