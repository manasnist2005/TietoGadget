using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DD4T.Mvc.Controllers;
using System.Web.Mvc;
using DD4T.ContentModel;
using DD4T.ContentModel.Exceptions;
using System.Configuration;
using System.Xml;
using TietoGadget.Models;
using Tridion.ContentDelivery.Web.WAI;


namespace TietoGadget.Controllers
{
    public class ComponentController : TridionControllerBase
    {
        protected override ViewResult GetView(IComponentPresentation componentPresentation)
        {
            if (!componentPresentation.ComponentTemplate.MetadataFields.ContainsKey("view"))
            {
                throw new ConfigurationErrorsException("no view configured for component template " + componentPresentation.ComponentTemplate.Id);
            }
            if (!CheckTargetGroup(componentPresentation))
                return View("blank");
            else
            {
                string viewName = componentPresentation.ComponentTemplate.MetadataFields["view"].Value;
                return View(viewName, componentPresentation);
            }
        }
        /// <summary>
        /// Extract the conditions from the Component Presentation and checks if it suits for a user or not
        /// </summary>
        /// <param name="componentPresentation"></param>
        /// <returns></returns>
        private bool CheckTargetGroup(IComponentPresentation componentPresentation)
        {
            bool chkGroup = true;
            if (componentPresentation.Conditions.Count > 0)
            {
                List<Condition> lstCondition = new List<Condition>();
                foreach (Condition cpConditions in componentPresentation.Conditions)
                {
                    lstCondition.Add(cpConditions);
                }
                return CheckConditions(lstCondition);
            }
            return chkGroup;
        }
        /// <summary>
        /// Checks for 3 kinds of Conditions and returns the if it satisfies or not..
        /// </summary>
        /// <param name="conditionList"></param>
        /// <returns></returns>
        private bool CheckConditions(List<Condition> conditionList)
        {
            bool chkGroup = true;
            foreach (Condition cpConditions in conditionList)
            {
                if (cpConditions is CustomerCharacteristicCondition)
                {
                    var ccCondition = (CustomerCharacteristicCondition)cpConditions;
                    var node = (XmlNode[])ccCondition.Value;
                    string userPreferenceValue = UserPreferenceValue(ccCondition.Name.ToUpper());
                    if (!CCOpertators(ccCondition.Operator, userPreferenceValue.ToString(), node[2].Value.ToString()))
                        return false;
                }
                if (cpConditions is KeywordCondition)
                {
                    if(Session["WAIUser"] != null)
                    {
                    var kwCondition = (KeywordCondition)cpConditions;
                    var value = Convert.ToInt32(((XmlNode[])kwCondition.Value)[2].Value.ToString());
                    var keyword = kwCondition.Keyword.Title;
                    string userdetails = Session["WAIUser"].ToString();
                    var usr = UserManager.CreateUser(Convert.ToInt32(userdetails.Split('_')[0].ToString()), Convert.ToInt32(userdetails.Split('_')[1].ToString()), "");
                    Tridion.ContentDelivery.Web.WAI.TrackingKeys trackingKey = new TrackingKeys(usr);
                    int userPreferenceValue = trackingKey.GetKey(keyword);
                    if (!KWOpertators(kwCondition.Operator, userPreferenceValue, value))
                        return false;
                    }
                }

                if (cpConditions is TargetGroupCondition)
                {
                    var TGConditions = ((TargetGroupCondition)cpConditions).TargetGroup.Conditions;
                    CheckConditions(TGConditions);
                }
            }
            return chkGroup;

        }
        /// <summary>
        /// Compares for different types of condition operator for customer characterstic
        /// </summary>
        /// <param name="Operator"></param>
        /// <param name="operand1"></param>
        /// <param name="operand2"></param>
        /// <returns></returns>
        private bool CCOpertators(ConditionOperator Operator, string operand1, string operand2)
        {
            switch (Operator)
            {
                case ConditionOperator.StringEquals: return operand1.ToString().Equals(operand2.ToString());
                case ConditionOperator.Contains: return operand1.ToString().Contains(operand2.ToString());
                case ConditionOperator.StartsWith: return operand1.ToString().StartsWith(operand2.ToString());
                case ConditionOperator.EndsWith: return operand1.ToString().EndsWith(operand2.ToString());
                case ConditionOperator.Equals: return (Convert.ToInt32(operand1) == Convert.ToInt32(operand2));
                case ConditionOperator.NotEqual: return (Convert.ToInt32(operand1) != Convert.ToInt32(operand2));
                case ConditionOperator.GreaterThan: return (Convert.ToInt32(operand1) > Convert.ToInt32(operand2));
                case ConditionOperator.LessThan: return (Convert.ToInt32(operand1) < Convert.ToInt32(operand2));
                default: return false;
            }
        }

        /// <summary>
        /// Compares for different types of condition operator for customer characterstic
        /// </summary>
        /// <param name="Operator"></param>
        /// <param name="operand1"></param>
        /// <param name="operand2"></param>
        /// <returns></returns>
        private bool KWOpertators(NumericalConditionOperator Operator, int operand1, int operand2)
        {
            switch (Operator)
            {              
                case NumericalConditionOperator.Equals: return (operand1 == operand2);
                case NumericalConditionOperator.NotEqual: return (operand1 != operand2);
                case NumericalConditionOperator.GreaterThan: return (operand1 > operand2);
                case NumericalConditionOperator.LessThan: return (operand1 < operand2);
                default: return false;
            }
        }
        /// <summary>
        /// Get the user preference value from the User session
        /// </summary>
        /// <param name="userPreference"></param>
        /// <returns>user preference value</returns>
        private string UserPreferenceValue(string userPreference)
        {
            if (Session["User"] != null)
            {
                TietoGadget.Models.User usr = Session["User"] as TietoGadget.Models.User;
                switch (userPreference)
                {
                    case "AGE": return usr.Age.ToString();
                    case "CITY": return usr.City.ToString();
                    case "COMPANY": return usr.Company.ToString();
                    case "STATE": return usr.State.ToString();
                    case "WORKING YEAR": return usr.Working_years.ToString();
                    case "ZIP": return usr.Zip.ToString();
                    default: return string.Empty;
                }
            }
            else
                return string.Empty;
        }

    }
}
