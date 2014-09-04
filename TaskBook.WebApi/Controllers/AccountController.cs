﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using TaskBook.DataAccessLayer.Exceptions;
using TaskBook.Services.Interfaces;
using TaskBook.DomainModel.ViewModels;
using TaskBook.WebApi.Attributes;
using TaskBook.DomainModel;
using TaskBook.Services;
using System.Web;
using TaskBook.Services.AuthManagers;
using Microsoft.Practices.Unity;

namespace TaskBook.WebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private readonly IUserService _userService;
        private TbUserManager _userManager;
        private readonly bool _softDeleted = false;

        public AccountController(IUserService userService, TbUserManager userManager)
        {
            _userService = userService;
            _userManager = userManager;
            var url = HttpContext.Current.Request.Url;
            _userService.Host = string.Format("{0}://{1}:{2}", url.Scheme, url.Host, url.Port);
        }

        [InjectionConstructor]
        public AccountController(IUserService userService)
        {
            _userService = userService;
            _userService.UserManager = UserManager;
            var url = HttpContext.Current.Request.Url;
            _userService.Host = string.Format("{0}://{1}:{2}", url.Scheme, url.Host, url.Port);
        }

        public TbUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<TbUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        // GET api/Account/GetUserByUserName/{userName}
        [Route("GetUserByUserName/{userName}")]
        [ResponseType(typeof(TbUserRoleVm))]
        public IHttpActionResult GetUserByUserName(string userName)
        {
            try
            {
                var userVm = _userService.GetUserByUserName(userName);
                if(userVm == null)
                {
                    return BadRequest(string.Format("Unable to return data for user '{0}'.", userName));
                }
                return Ok(userVm);
            }
            catch(DataAccessReaderException ex)
            {
                return BadRequest(string.Format("{0}: {1}", ex.Message, ex.InnerException.Message));
            }
        }

        // GET api/Account/GetUsersWithRolesByProjectId/{projectId}
        [Route("GetUsersWithRolesByProjectId/{projectId:long}")]
        [ResponseType(typeof(IQueryable<TbUserRoleVm>))]
        public IHttpActionResult GetUsersWithRolesByProjectId(long projectId)
        {
            try
            {
                var users = _userService.GetUsersWithRolesByProjectId(projectId);
                if(users == null)
                {
                    return BadRequest(string.Format("Unable to return users for the project with ID '{0}'.", projectId));
                }
                return Ok(users);
            }
            catch(DataAccessReaderException ex)
            {
                return BadRequest(string.Format("{0}: {1}", ex.Message, ex.InnerException.Message));
            }
        }

        // GET api/Account/GetUsersByProjectId/{projectId}
        [Route("GetUsersByProjectId/{projectId:long}")]
        [ResponseType(typeof(IQueryable<UserProjectVm>))]
        public IHttpActionResult GetUsersByProjectId(long projectId)
        {
            try
            {
                var users = _userService.GetUsersByProjectId(projectId);
                if(users == null)
                {
                    return BadRequest(string.Format("Unable to return users for the project with ID '{0}'.", projectId));
                }
                return Ok(users);
            }
            catch(DataAccessReaderException ex)
            {
                return BadRequest(string.Format("{0}: {1}", ex.Message, ex.InnerException.Message));
            }
        }

        // POST api/Account/ForgotPassword
        [Route("ForgotPassword")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IHttpActionResult> ForgotPassword(ForgotPasswordVm model)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _userService.ForgotPassword(model);
            }
            catch(TbIdentityException ex)
            {
                return BadRequest(string.Format("{0}: {1}", ex.Message, ex.InnerException.Message));
            }
            catch(ArgumentNullException ex)
            {
                return BadRequest(string.Format("{0}: {1}", ex.Message, ex.InnerException.Message));
            }
            return Ok();
        }


        // POST api/Account/ResetPassword
        [Route("ResetPassword")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IHttpActionResult> ResetPassword(ResetPasswordVm model)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _userService.ResetPassword(model);
            }
            catch(TbIdentityException ex)
            {
                return BadRequest(string.Format("{0}: {1}", ex.Message, ex.InnerException.Message));
            }
            catch(Exception ex)
            {
                string s1 = ex.Message;
            }
            return Ok();
        }

        // POST api/Account/AddUser
        [Route("AddUser")]
        [AuthorizeRoles(RoleKey.Admin, RoleKey.Manager)]
        [HttpPost]
        public async Task<IHttpActionResult> AddUserAsync(TbUserRoleVm userModel)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _userService.AddUserAsync(userModel);
            }
            catch(DataAccessLayerException ex)
            {
                return BadRequest(string.Format("{0}: {1}", ex.Message, ex.InnerException.Message));
            }
            catch(TbIdentityException ex)
            {
                return GetErrorResult(ex.TbIdentityResult);
            }
            catch(Exception ex)
            {
                string error = ex.Message;
            }
            return Ok();
        }

        // PUT api/Account/UpdateUser/{id}
        [Route("UpdateUser/{id}")]
        [HttpPut]
        public IHttpActionResult UpdateUser(string id, TbUserRoleVm userVm)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _userService.UpdateUser(id, userVm);
            }
            catch(TbIdentityException ex)
            {
                return GetErrorResult(ex.TbIdentityResult);
            }
            catch(Exception ex)
            {
                return BadRequest(string.Format("{0}: {1}", ex.Message, ex.InnerException.Message));
            }
           
            return Ok();
        }

        // DELETE api/Account/DeleteUser/{id}
        [Route("DeleteUser/{id}")]
        [HttpDelete]
        [AuthorizeRoles(RoleKey.Admin, RoleKey.Manager)]
        public IHttpActionResult DeleteUser(string id)
        {
            try
            {
                _userService.DeleteUser(id, _softDeleted);
            }
            catch(DataAccessLayerException ex)
            {
                return BadRequest(string.Format("{0}: {1}", ex.Message, ex.InnerException.Message));
            }
            catch(DataAccessReaderException ex)
            {
                return BadRequest(string.Format("{0}: {1}", ex.Message, ex.InnerException.Message));
            }
            catch(TbIdentityException ex)
            {
                return GetErrorResult(ex.TbIdentityResult);
            }
            catch(Exception ex)
            {
                return BadRequest(string.Format("{0}: {1}", ex.Message, ex.InnerException.Message));
            }

            return Ok();
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if(result == null)
            {
                return InternalServerError();
            }
            else // !result.Succeeded
            {
                if(result.Errors != null)
                {
                    foreach(string error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
                return BadRequest(ModelState);
            }
        }

        protected override void Dispose(bool disposing)
        {
            _userService.Dispose();
            base.Dispose(disposing);
        }
    }
}
