using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Intelligent.OTC.WebApi.Core
{
    public class AntiSqlInjectFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            var actionParameters = filterContext.ActionDescriptor.GetParameters();

            var actionArguments = filterContext.ActionArguments;

            foreach (var p in actionParameters)
            {
                var value = filterContext.ActionArguments[p.ParameterName];

                var pType = p.ParameterType;

                if (value == null)
                {
                    continue;
                }
                //如果不是值类型或接口，不需要过滤
                if (!pType.IsClass) continue;

                if (value is string)
                {
                    //对string类型过滤
                    filterContext.ActionArguments[p.ParameterName] = AntiSqlInject.Instance.GetSafetySql(value.ToString());
                }
                else
                {
                    //是一个class，对class的属性中，string类型的属性进行过滤
                    var properties = pType.GetProperties();
                    foreach (var pp in properties)
                    {
                        try
                        {
                            var temp = pp.GetValue(value, null);
                            if (temp == null)
                            {
                                continue;
                            }
                            try
                            {
                                pp.SetValue(value, temp is string ? AntiSqlInject.Instance.GetSafetySql(temp.ToString()) : temp, null);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }

        }
    }
}