#if FAKE_XRM_EASY_9

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace FakeXrmEasy.Extensions
{
    /// <summary>
    /// Extension methods for OptionSetValue Collection
    /// </summary>
    public static class OptionSetValueCollectionExtensions
    {
        
        /// <summary>
        /// Converts current OptionSetValueCollection to a HashSet&lt;int&gt; values
        /// </summary>
        /// <param name="input"></param>
        /// <param name="isOptionSetValueCollectionAccepted"></param>
        /// <returns></returns>
        public static HashSet<int> ConvertToHashSetOfInt(object input, bool isOptionSetValueCollectionAccepted)
        {
            HashSet<int> set = null;

            var faultReason = $"The formatter threw an exception while trying to deserialize the message: There was an error while trying to deserialize parameter" +
                $" http://schemas.microsoft.com/xrm/2011/Contracts/Services:query. The InnerException message was 'Error in line 1 position 8295. Element " +
                $"'http://schemas.microsoft.com/2003/10/Serialization/Arrays:anyType' contains data from a type that maps to the name " +
                $"'http://schemas.microsoft.com/xrm/2011/Contracts:{input?.GetType()}'. The deserializer has no knowledge of any type that maps to this name. " +
                $"Consider changing the implementation of the ResolveName method on your DataContractResolver to return a non-null value for name " +
                $"'{input?.GetType()}' and namespace 'http://schemas.microsoft.com/xrm/2011/Contracts'.'.  Please see InnerException for more details.";

            if (input is int)
            {
                set = new HashSet<int>(new int[] {(int)input});
            }
            else if (input is string)
            {
                set = new HashSet<int>(new int[] { int.Parse(input as string) });
            }
            else if (input is int[])
            {
                set = new HashSet<int>(input as int[]);
            }
            else if (input is string[])
            {
                set = new HashSet<int>((input as string[]).Select(s => int.Parse(s)));
            }
            else if (input is DataCollection<object>)
            {
                var collection = input as DataCollection<object>;

                if (collection.All(o => o is int))
                {
                    set = new HashSet<int>(collection.Cast<int>());
                }
                else if (collection.All(o => o is string))
                {
                    set = new HashSet<int>(collection.Select(o => int.Parse(o as string)));
                }
                else if (collection.Count == 1 && collection[0] is int[])
                {
                    set = new HashSet<int>(collection[0] as int[]);
                }
                else if (collection.Count == 1 && collection[0] is string[])
                {
                    set = new HashSet<int>((collection[0] as string[]).Select(s => int.Parse(s)));
                }
                else
                {
                    throw FakeOrganizationServiceFaultFactory.New(faultReason);
                }
            }
            else if (isOptionSetValueCollectionAccepted && input is OptionSetValueCollection)
            {
                set = new HashSet<int>((input as OptionSetValueCollection).Select(osv => osv.Value));
            }
            else
            {
                throw FakeOrganizationServiceFaultFactory.New(faultReason);
            }

            return set;
        }
    }
}
#endif