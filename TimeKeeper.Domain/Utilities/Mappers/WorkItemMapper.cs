using TimeKeeper.Domain.Enums;

namespace TimeKeeper.Domain.Utilities.Mappers
{
    public static class WorkItemMapper
    {
        /// <summary>
        /// Returns a new DataAccess Entity with the values of the domain Model mapped to it.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static DataAccess.Entities.WorkItem MapDomainToEntity(this Models.WorkItem model)
        {
            DataAccess.Entities.WorkItem entity = new()
            {
                ActivityNumber = model.ActivityNumber,
                CreatedBy = model.CreatedBy,
                CreatedDate = model.CreatedDate.ConvertLocalToUtc(),
                Id = model.Id,
                UpdatedBy = model.UpdatedBy,
                UpdatedDate = model.UpdatedDate.ConvertLocalToUtc(),
                IsOpen = model.IsOpen,
                IsInReview = model.IsInReview,
                ProjectId = model.ProjectId,
                Title = model.Title,
                Description = model.Description,
                IsBillable = model.IsBillable,
                WorkItemType = (int)(model.WorkItemType ?? default),
            };

            if (model.TimeEntries != null)
            {
                foreach (var entryModel in model.TimeEntries)
                {
                    DataAccess.Entities.TimeEntry timeEntry = entryModel.MapDomainToEntity();
                    entity.TimeEntries.Add(timeEntry);
                }
            }

            return entity;
        }

        /// <summary>
        /// Updates the values of an existing entity reference with the values from the Domain Model.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entity"></param>
        public static void MapDomainToEntity(this Models.WorkItem model, ref DataAccess.Entities.WorkItem entity)
        {
            entity.ActivityNumber = model.ActivityNumber;
            entity.CreatedBy = model.CreatedBy;
            entity.CreatedDate = model.CreatedDate.ConvertLocalToUtc();
            entity.UpdatedBy = model.UpdatedBy;
            entity.UpdatedDate = model.UpdatedDate.ConvertLocalToUtc();
            entity.IsOpen = model.IsOpen;
            entity.IsInReview = model.IsInReview;
            entity.ProjectId = model.ProjectId;
            entity.Title = model.Title;
            entity.Description = model.Description;
            entity.IsBillable = model.IsBillable;
            entity.WorkItemType = (int)(model.WorkItemType ?? default);

            //if(entity.TimeEntries != null)
            //{
            //    // If the TimeEntry entity does not exist in the model's TimeEntries then remove it
            //    // from the Entity.
            //    //
            //    for(var i = entity.TimeEntries.Count - 1; i >= 0; --i)
            //    {
            //        DataAccess.Entities.TimeEntry timeEntryEntity = entity.TimeEntries.ToArray()[i];

            //        if (model.TimeEntries.FirstOrDefault(e=>e.Id == model.Id) == null)
            //        {
            //            entity.TimeEntries.Remove(timeEntryEntity);
            //        }
            //    }

            //    // If there is an entryModel in the model that does not exist in the Entity then add it
            //    // to the Entity
            //    //
            //    foreach(Models.TimeEntry entryModel in model.TimeEntries)
            //    {
            //        if(entity.TimeEntries.FirstOrDefault(e=>e.Id == entryModel.Id) == null)
            //        {
            //            DataAccess.Entities.TimeEntry newEntry = entryModel.MapDomainToEntity();
            //            entity.TimeEntries.Add(newEntry);
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Returns a new Domain model with the values of the Entity mapped to it.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Models.WorkItem MapEntityToDomain(this DataAccess.Entities.WorkItem entity)
        {
            var model = new Models.WorkItem()
            {
                ActivityNumber = entity.ActivityNumber,
                CreatedBy = entity.CreatedBy,
                CreatedDate = entity.CreatedDate.ConvertUtcToLocal(),
                Id = entity.Id,
                UpdatedBy = entity.UpdatedBy,
                UpdatedDate = entity.UpdatedDate.ConvertUtcToLocal(),
                IsOpen = entity.IsOpen,
                IsInReview = entity.IsInReview,
                ProjectId = entity.ProjectId,
                Title = entity.Title,
                Description = entity.Description,
                IsBillable = entity.IsBillable,
                WorkItemType = (WorkItemType)entity.WorkItemType,
            };

            if(entity.Project != null)
            {
                model.Project = entity.Project.MapEntityToDomain();
            }

            //if (entity.TimeEntries != null)
            //{
            //    model.TimeEntries = new List<Models.TimeEntry>();
            //    foreach (DataAccess.Entities.TimeEntry entry in entity.TimeEntries)
            //    {
            //        Models.TimeEntry entryModel = entry.MapEntityToDomain();
            //        entryModel.TakeSnapshot();
            //        model.TimeEntries.Add(entryModel);
            //    }
            //}

            return model;
        }
    }
}