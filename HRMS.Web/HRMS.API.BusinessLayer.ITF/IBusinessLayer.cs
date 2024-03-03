using HRMS.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.API.BusinessLayer.ITF
{
    public interface IBusinessLayer
    {
        LoginUser LoginUser(LoginUser loginUser);
    }
}
