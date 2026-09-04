using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using TimeKeeper.Domain.Commands;
using TimeKeeper.Domain.Models;
using TimeKeeper.Domain.Queries;
using TimeKeeper.Domain.Services.Interfaces;

namespace TimeKeeper.Domain.Services
{

    public class LaborerService : ILaborerService
    {
        #region fields

        IMediator? _mediator = null;

        #endregion fields

        #region properties
        #endregion properties

        #region data
        #endregion data

        #region ctor

        public LaborerService(ProjectServiceOptions options)
        {
            this._mediator = options.Mediator;
        }

        #endregion ctor

        #region private
        #endregion private

        #region public

        /// <summary>
        /// Inserts a new Laborer record and registers a new user in the database
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<Models.Laborer?> CreateUser(Models.Laborer model)
        {
            Models.Laborer? result = (this._mediator != null) ? await this._mediator.Send(new AddLaborer(model)) : null;
            return result;
        }

        /// <summary>
        /// Returns a Laborer/User record which is associated with the <paramref name="password"/> 
        /// and <paramref name="username"/> arguments. Think ... Login
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<Models.Laborer?> GetFromCredentials(string username, string password)
        {
            // If neither credential is supplied then return null
            //
            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            Models.Laborer? result = (this._mediator != null) ? await this._mediator.Send(new GetLaborerByCredentials(username, password)) : null;
            return result;
        }

        /// <summary>
        /// Returns the Laborer/User record associated with the <paramref name="laborerID"/> argument.
        /// </summary>
        /// <param name="laborerID"></param>
        /// <returns></returns>
        public async Task<Models.Laborer?> GetFromId(int laborerID)
        {
            Models.Laborer? result = (this._mediator != null) ? await this._mediator.Send(new GetLaborerByID(laborerID)) : null;
            return result;
        }

        /// <summary>
        /// Updates the password for the <paramref name="model"/> argument.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> UpdatePassword(Models.Laborer model)
        {
            Boolean result = (this._mediator != null) ? await this._mediator.Send(new ChangePassword(model)) : false;
            return result;
        }

        /// <summary>
        /// Updates a Laborer/User record.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> UpdateUser(Models.Laborer model)
        {
            Boolean result = (this._mediator != null) ? await this._mediator.Send(new UpdateLaborer(model)) : false;
            return result;
        }

        #endregion public
    }

    public record LaborerServiceOptions(IMediator Mediator);

}
