using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TimeKeeper.DataAccess.Entities;
using TimeKeeper.Domain.Exceptions;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Security;
using TimeKeeper.Domain.Utilities.Mappers;

namespace TimeKeeper.Domain.Commands
{
    public class LaborerCommandHandler(TimeKeeperDbContext ctx) :
        IRequestHandler<AddLaborer, Models.Laborer>,
        IRequestHandler<UpdateLaborer, Boolean>,
        IRequestHandler<DeleteLaborer, Boolean>,
        IRequestHandler<ChangePassword, Boolean>
    {
        readonly TimeKeeperDbContext _dbCtx = ctx;

        /// <summary>
        ///  Update the password of the Laborer identified by the ID contained in the <paramref name="request"/> 
        ///  argument.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Handle(ChangePassword request, CancellationToken cancellationToken)
        {
            bool isSuccess = false;

            // First we need to make sure that a record exists that matches with the username.
            //
            DataAccess.Entities.Laborer? entity = await this._dbCtx.Laborers.FirstOrDefaultAsync(e => e.Username.ToLower() == request.Model.Username.ToLower());

            if (entity == null)
            {
                string errMsg = $"Laborer with username {request.Model.Username} not found";
                throw new LaborerNotFoundException(errMsg);
            }

            // If the password is valid then hash the new password and set it to the entity and
            // save it.
            //
            if(request.Model.PasswordIsValid())
            {
                string hashedPassword = PasswordUtilities.HashPassword(request.Model.Username, request.Model.Password);
                entity.Password = hashedPassword;
                await this._dbCtx.SaveChangesAsync(cancellationToken);
                isSuccess = true;
            }

            return isSuccess;
        }

        /// <summary>
        /// Permanently removes an laborer record and associated records permanently from the database.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> Handle(DeleteLaborer request, CancellationToken cancellationToken)
        {
            bool result = false;

            DataAccess.Entities.Laborer? entity = await this._dbCtx.Laborers.FirstOrDefaultAsync(e => e.Id == request.LaborerID);

            if(entity != null)
            {
                // If more than 0 rows were affected then return TRUE
                //
                this._dbCtx.Laborers.Remove(entity);
                result = (await this._dbCtx.SaveChangesAsync(cancellationToken)) > 0;
            }

            return result;
        }

        /// <summary>
        /// Inserts a new Laborer entry in the database.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="DuplicateUsernameException"></exception>
        /// <exception cref="DuplicateEmailException"></exception>
        public async Task<Models.Laborer> Handle(AddLaborer request, CancellationToken cancellationToken)
        {
            Models.Laborer? result = null;

            // If the Model fails validation it will throw a PassworfValidationException
            //
            if (request.Model.PasswordIsValid())
            {
                // Make sure that the Username is not already being used.
                //
                var existingEntities = await this._dbCtx.Laborers.FirstOrDefaultAsync(e => e.Username.ToLower() == request.Model.Username.ToLower());
                if (existingEntities != null)
                {
                    throw new DuplicateUsernameException($"The username is not available.", request.Model);
                }

                // Make sure that the email address is not already being used.
                //
                existingEntities = await this._dbCtx.Laborers.FirstOrDefaultAsync(e => e.Email.ToLower() == request.Model.Email.ToLower());
                if (existingEntities != null)
                {
                    throw new DuplicateEmailException($"The email is already in use.", request.Model);
                }

                // Map the domain model to a new database entity
                //
                DataAccess.Entities.Laborer entity = request.Model.MapDomainToEntity();

                // update the timestamps and hash the password
                //
                entity.UpdatedDate = entity.CreatedDate = DateTime.UtcNow;
                entity.Password = PasswordUtilities.HashPassword(request.Model.Username, request.Model.Password);

                await this._dbCtx.Laborers.AddAsync(entity, cancellationToken);
                this._dbCtx.SaveChanges();

                // return the newly created object including it's database ID.
                //
                result = entity.MapEntityToDomain();
            }
            else
            {
                result = request.Model;
            }

            return result;
        }

        /// <summary>
        /// Updates an existing Laborer record with the values in the Domain Model stored in the <paramref name="request"/> argument.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> Handle(UpdateLaborer request, CancellationToken cancellationToken)
        {
            bool result = false;

            // Get the entity to be updated
            //
            DataAccess.Entities.Laborer? entity = await this._dbCtx.Laborers.FirstOrDefaultAsync(e=> e.Id == request.Model.Id);

            if(entity != null)
            {
                request.Model.MapDomainToEntity(ref entity);
                entity.UpdatedDate = DateTime.UtcNow;
                
                result = this._dbCtx.SaveChanges() > 0;
            }

            return result;
        }
    }

    /// <summary>
    /// Inserts the <paramref name="Model"/> argument into the Laborer table in the database
    /// </summary>
    /// <param name="Model"></param>
    public record AddLaborer(Models.Laborer Model) : IRequest<Models.Laborer>;

    /// <summary>
    /// Changes the Password of the Laborer associated with the <paramref name="LaborerID"/> argument.
    /// </summary>
    /// <param name="LaborerID"></param>
    /// <param name="Password"></param>
    /// <param name="PasswordConfirm"></param>
    public record ChangePassword(Models.Laborer Model) : IRequest<bool>;

    /// <summary>
    /// Permanently removes an laborer record associated with the <paramref name="LaborerID"/> 
    /// argument and associated records permanently from the database.
    /// </summary>
    /// <param name="LaborerID"></param>
    public record DeleteLaborer(int LaborerID) : IRequest<bool>;

    /// <summary>
    /// Updates an existing Laborer record with the values in the <paramref name="Model"/> argument.
    /// </summary>
    /// <param name="Model"></param>
    public record UpdateLaborer(Models.Laborer Model) : IRequest<bool>;

}
