﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using TaskBook.DataAccessLayer;
using TaskBook.DomainModel;

namespace TaskBook.Services.AuthManagers
{
    public class TbRoleManager: RoleManager<TbRole>
    {
        public TbRoleManager(IRoleStore<TbRole, string> roleStore)
            : base(roleStore)
        {
        }

        public static TbRoleManager Create(IdentityFactoryOptions<TbRoleManager> options, IOwinContext context)
        {
            var roleManager = new TbRoleManager(new RoleStore<TbRole>(context.Get<TaskBookDbContext>()));
            return roleManager;
        }
    }
}
