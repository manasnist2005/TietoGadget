using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Tridion.ContentDelivery.DynamicContent.Query;
using Tridion.ContentDelivery.DynamicContent;
using Tridion.ContentDelivery.Taxonomies;

namespace TietoGadget.Helpers
{
    public class BrokerQueryHelper
    {
        public static List<string> GetProductIntroComponentList(string pageCategory)
        {
            //We create a list of strings which will be the data source of our repeater.
            List<string> itemsList = new List<string>();

            try
            {
                //Here is our first criteria - ItemSchemaCriteria. Here we say to the query that we need all the items with the specified schema id.
                //We have hardcoded the id of the schema for this example. 
                ItemSchemaCriteria schemaCriteria = new ItemSchemaCriteria(4604);

                //We might also want to make a Type criteria. For example, "Is the item type 'Component'?" criteria.
                //However, we've already set a schema criteria and this means that we will only have items of type Component returned. 
                //But we may need the ItemTypeCriteria for other purposes.
                //We have hardcoded the id of the type for this example.
                ItemTypeCriteria typeCriteria = new ItemTypeCriteria(16);

                //We might want to specify the publication as well. For doing so, we can use the PublicationCriteria.
                //We have hardcoded the id of the publication for this example.
                PublicationCriteria publicationCriteria = new PublicationCriteria(2033);

                // Tieto_Gadget_productcategory : is the category and pageCategory: is the keyword attached to the page
                KeywordCriteria kwdCriteria = new KeywordCriteria("Tieto_Gadget_productcategory", pageCategory);
                //If we need to specify a limit of the listed items, we may use LimitFilter. In this case we want 4 items to be displayed so we set the parameter to 4.
                LimitFilter limitFilter = new LimitFilter(4);
                 

                //We create a list of Criteria and add the criteria we specified above to this list.
                List<Criteria> allCriteria = new List<Criteria>();
                allCriteria.Add(schemaCriteria);
                allCriteria.Add(typeCriteria);
                allCriteria.Add(publicationCriteria);
                allCriteria.Add(kwdCriteria);

                //If we need to sort our results (for example if we want to display the latest items, then we can sort the results by date)
                //we can use a SortParameter. In this case we take the value of a Metadata Field called "Date" and we sort the results by it.
                //CustomMetaKeyColumn customMetaKeyColumnDate = new CustomMetaKeyColumn("Date", MetadataType.DATE);
                //SortParameter sortParameter = new SortParameter(customMetaKeyColumnDate, SortParameter.Descending);

                //Create the query.
                Query queryProduct = new Query();
                //Add all criteria.
                queryProduct.Criteria = CriteriaFactory.And(allCriteria.ToArray());
                //Set the limit filter.
                //queryProduct.SetResultFilter(limitFilter);
                //If we have a sorting parameter we can add it like this.
                //QueryNews.AddSorting(sortParameter);

                //Execute the query.
                String[] ItemResults = queryProduct.ExecuteQuery();

                //Go through the query result entries.
                foreach (String item in ItemResults)
                {
                    //Get the component presentation
                    //ComponentPresentationAssembler ass = new ComponentPresentationAssembler();
                    //Get the content of the component presentation
                    //string content = ass.GetContent(news, TemplateId);
                    //Add the content to the data source of the repeater
                    itemsList.Add(item);
                }

                return itemsList;

            }
            catch (Exception ex)
            {
                return itemsList;
                //TODO: Add error logging
            }
        }

        public static void getKeyword()
        {
            Query keywordQuery = new Query();
           
            CategoryCriteria categoryCriteria = new CategoryCriteria("Subscription_Category");
            ItemTypeCriteria typeCriteria = new ItemTypeCriteria(1024);
            
            List<Criteria> allCriteria = new List<Criteria>();
            allCriteria.Add(categoryCriteria);
            allCriteria.Add(typeCriteria);
            keywordQuery.Criteria = CriteriaFactory.And(allCriteria.ToArray());

            String[] ItemResults = keywordQuery.ExecuteQuery();
        }
    }
}