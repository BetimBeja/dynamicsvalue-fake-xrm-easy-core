using FakeXrmEasy.Abstractions;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Xrm.Sdk.Query;
using FakeXrmEasy.Models;
using System.Linq;
using Microsoft.Xrm.Sdk;
using FakeXrmEasy.Extensions;
using System;

namespace FakeXrmEasy.Query
{
    public static partial class ConditionExpressionExtensions
    {
        internal static BinaryExpression TranslateMultipleConditionExpressions(this List<ConditionExpression> conditions, QueryExpression qe, IXrmFakedContext context, string sEntityName, LogicalOperator op, ParameterExpression entity, bool bIsOuter)
        {
            BinaryExpression binaryExpression = null;  //Default initialisation depending on logical operator
            if (op == LogicalOperator.And)
                binaryExpression = Expression.And(Expression.Constant(true), Expression.Constant(true));
            else
                binaryExpression = Expression.Or(Expression.Constant(false), Expression.Constant(false));

            foreach (var c in conditions)
            {
                var cEntityName = sEntityName;
                //Create a new typed expression 
                var typedExpression = new TypedConditionExpression(c);
                typedExpression.IsOuter = bIsOuter;

                string sAttributeName = c.AttributeName;

                //Find the attribute type if using early bound entities
                if (context.ProxyTypesAssemblies.Count() > 0)
                {

                    if (c.EntityName != null)
                        cEntityName = qe.GetEntityNameFromAlias(c.EntityName);
                    else
                    {
                        if (c.AttributeName.IndexOf(".") >= 0)
                        {
                            var alias = c.AttributeName.Split('.')[0];
                            cEntityName = qe.GetEntityNameFromAlias(alias);
                            sAttributeName = c.AttributeName.Split('.')[1];
                        }
                    }

                    var earlyBoundType = context.FindReflectedType(cEntityName);
                    if (earlyBoundType != null)
                    {
                        typedExpression.AttributeType = context.FindReflectedAttributeType(earlyBoundType, cEntityName, sAttributeName);

                        // Special case when filtering on the name of a Lookup
                        if (typedExpression.AttributeType == typeof(EntityReference) && sAttributeName.EndsWith("name"))
                        {
                            var realAttributeName = c.AttributeName.Substring(0, c.AttributeName.Length - 4);

                            if (earlyBoundType.GetEarlyBoundTypeAttribute(sAttributeName) == null 
                                && earlyBoundType.GetEarlyBoundTypeAttribute(realAttributeName) != null 
                                && earlyBoundType.GetEarlyBoundTypeAttribute(realAttributeName).PropertyType == typeof(EntityReference))
                            {
                                // Need to make Lookups work against the real attribute, not the "name" suffixed attribute that doesn't exist
                                c.AttributeName = realAttributeName;
                            }
                        }
                    }
                }

                typedExpression.ValidateSupportedTypedExpression();

                //Build a binary expression  
                if (op == LogicalOperator.And)
                {
                    binaryExpression = Expression.And(binaryExpression, typedExpression.TranslateConditionExpression(qe, context, entity));
                }
                else
                    binaryExpression = Expression.Or(binaryExpression, typedExpression.TranslateConditionExpression(qe, context, entity));
            }

            return binaryExpression;
        }

        internal static Expression TranslateConditionExpression(this TypedConditionExpression c, QueryExpression qe, IXrmFakedContext context, ParameterExpression entity)
        {
            Expression attributesProperty = Expression.Property(
                entity,
                "Attributes"
                );


            //If the attribute comes from a joined entity, then we need to access the attribute from the join
            //But the entity name attribute only exists >= 2013
#if FAKE_XRM_EASY_2013 || FAKE_XRM_EASY_2015 || FAKE_XRM_EASY_2016 || FAKE_XRM_EASY_365 || FAKE_XRM_EASY_9
            string attributeName = "";

            //Do not prepend the entity name if the EntityLogicalName is the same as the QueryExpression main logical name

            if (!string.IsNullOrWhiteSpace(c.CondExpression.EntityName) && !c.CondExpression.EntityName.Equals(qe.EntityName))
            {
                attributeName = c.CondExpression.EntityName + "." + c.CondExpression.AttributeName;
            }
            else
                attributeName = c.CondExpression.AttributeName;

            Expression containsAttributeExpression = Expression.Call(
                attributesProperty,
                typeof(AttributeCollection).GetMethod("ContainsKey", new Type[] { typeof(string) }),
                Expression.Constant(attributeName)
                );

            Expression getAttributeValueExpr = Expression.Property(
                            attributesProperty, "Item",
                            Expression.Constant(attributeName, typeof(string))
                            );
#else
            Expression containsAttributeExpression = Expression.Call(
                attributesProperty,
                typeof(AttributeCollection).GetMethod("ContainsKey", new Type[] { typeof(string) }),
                Expression.Constant(c.CondExpression.AttributeName)
                );

            Expression getAttributeValueExpr = Expression.Property(
                            attributesProperty, "Item",
                            Expression.Constant(c.CondExpression.AttributeName, typeof(string))
                            );
#endif



            Expression getNonBasicValueExpr = getAttributeValueExpr;

            Expression operatorExpression = null;

            switch (c.CondExpression.Operator)
            {
                case ConditionOperator.Equal:
                case ConditionOperator.Today:
                case ConditionOperator.Yesterday:
                case ConditionOperator.Tomorrow:
                case ConditionOperator.EqualUserId:
                    operatorExpression = TranslateConditionExpressionEqual(context, c, getNonBasicValueExpr, containsAttributeExpression);
                    break;

                case ConditionOperator.NotEqualUserId:
                    operatorExpression = Expression.Not(TranslateConditionExpressionEqual(context, c, getNonBasicValueExpr, containsAttributeExpression));
                    break;

                case ConditionOperator.EqualBusinessId:
                    operatorExpression = TranslateConditionExpressionEqual(context, c, getNonBasicValueExpr, containsAttributeExpression);
                    break;

                case ConditionOperator.NotEqualBusinessId:
                    operatorExpression = Expression.Not(TranslateConditionExpressionEqual(context, c, getNonBasicValueExpr, containsAttributeExpression));
                    break;

                case ConditionOperator.BeginsWith:
                case ConditionOperator.Like:
                    operatorExpression = TranslateConditionExpressionLike(c, getNonBasicValueExpr, containsAttributeExpression);
                    break;

                case ConditionOperator.EndsWith:
                    operatorExpression = TranslateConditionExpressionEndsWith(c, getNonBasicValueExpr, containsAttributeExpression);
                    break;

                case ConditionOperator.Contains:
                    operatorExpression = TranslateConditionExpressionContains(c, getNonBasicValueExpr, containsAttributeExpression);
                    break;

                case ConditionOperator.NotEqual:
                    operatorExpression = Expression.Not(TranslateConditionExpressionEqual(context, c, getNonBasicValueExpr, containsAttributeExpression));
                    break;

                case ConditionOperator.DoesNotBeginWith:
                case ConditionOperator.DoesNotEndWith:
                case ConditionOperator.NotLike:
                case ConditionOperator.DoesNotContain:
                    operatorExpression = Expression.Not(TranslateConditionExpressionLike(c, getNonBasicValueExpr, containsAttributeExpression));
                    break;

                case ConditionOperator.Null:
                    operatorExpression = TranslateConditionExpressionNull(c, getNonBasicValueExpr, containsAttributeExpression);
                    break;

                case ConditionOperator.NotNull:
                    operatorExpression = Expression.Not(TranslateConditionExpressionNull(c, getNonBasicValueExpr, containsAttributeExpression));
                    break;

                case ConditionOperator.GreaterThan:
                    operatorExpression = TranslateConditionExpressionGreaterThan(c, getNonBasicValueExpr, containsAttributeExpression);
                    break;

                case ConditionOperator.GreaterEqual:
                    operatorExpression = TranslateConditionExpressionGreaterThanOrEqual(context, c, getNonBasicValueExpr, containsAttributeExpression);
                    break;

                case ConditionOperator.LessThan:
                    operatorExpression = TranslateConditionExpressionLessThan(c, getNonBasicValueExpr, containsAttributeExpression);
                    break;

                case ConditionOperator.LessEqual:
                    operatorExpression = TranslateConditionExpressionLessThanOrEqual(context, c, getNonBasicValueExpr, containsAttributeExpression);
                    break;

                case ConditionOperator.In:
                    operatorExpression = TranslateConditionExpressionIn(c, getNonBasicValueExpr, containsAttributeExpression);
                    break;

                case ConditionOperator.NotIn:
                    operatorExpression = Expression.Not(TranslateConditionExpressionIn(c, getNonBasicValueExpr, containsAttributeExpression));
                    break;

                case ConditionOperator.On:
                    operatorExpression = TranslateConditionExpressionEqual(context, c, getNonBasicValueExpr, containsAttributeExpression);
                    break;

                case ConditionOperator.NotOn:
                    operatorExpression = Expression.Not(TranslateConditionExpressionEqual(context, c, getNonBasicValueExpr, containsAttributeExpression));
                    break;

                case ConditionOperator.OnOrAfter:
                    operatorExpression = Expression.Or(
                               TranslateConditionExpressionEqual(context, c, getNonBasicValueExpr, containsAttributeExpression),
                               TranslateConditionExpressionGreaterThan(c, getNonBasicValueExpr, containsAttributeExpression));
                    break;
                case ConditionOperator.LastXHours:
                case ConditionOperator.LastXDays:
                case ConditionOperator.Last7Days:
                case ConditionOperator.LastXWeeks:
                case ConditionOperator.LastXMonths:
                case ConditionOperator.LastXYears:
                    operatorExpression = TranslateConditionExpressionLast(c, getNonBasicValueExpr, containsAttributeExpression);
                    break;

                case ConditionOperator.OnOrBefore:
                    operatorExpression = Expression.Or(
                                TranslateConditionExpressionEqual(context, c, getNonBasicValueExpr, containsAttributeExpression),
                                TranslateConditionExpressionLessThan(c, getNonBasicValueExpr, containsAttributeExpression));
                    break;

                case ConditionOperator.Between:
                    if (c.CondExpression.Values.Count != 2)
                    {
                        throw new Exception("Between operator requires exactly 2 values.");
                    }
                    operatorExpression = TranslateConditionExpressionBetween(c, getNonBasicValueExpr, containsAttributeExpression);
                    break;

                case ConditionOperator.NotBetween:
                    if (c.CondExpression.Values.Count != 2)
                    {
                        throw new Exception("Not-Between operator requires exactly 2 values.");
                    }
                    operatorExpression = Expression.Not(TranslateConditionExpressionBetween(c, getNonBasicValueExpr, containsAttributeExpression));
                    break;
#if !FAKE_XRM_EASY && !FAKE_XRM_EASY_2013
                case ConditionOperator.OlderThanXMinutes:
                case ConditionOperator.OlderThanXHours:
                case ConditionOperator.OlderThanXDays:
                case ConditionOperator.OlderThanXWeeks:
                case ConditionOperator.OlderThanXYears:                  
#endif
                case ConditionOperator.OlderThanXMonths:
                    operatorExpression = TranslateConditionExpressionOlderThan(c, getNonBasicValueExpr, containsAttributeExpression);
                    break;

                case ConditionOperator.NextXHours:               
                case ConditionOperator.NextXDays:                  
                case ConditionOperator.Next7Days:
                case ConditionOperator.NextXWeeks:                 
                case ConditionOperator.NextXMonths:                    
                case ConditionOperator.NextXYears:
                    operatorExpression = TranslateConditionExpressionNext(c, getNonBasicValueExpr, containsAttributeExpression);
                    break;
                case ConditionOperator.ThisYear:
                case ConditionOperator.LastYear:
                case ConditionOperator.NextYear:
                case ConditionOperator.ThisMonth:
                case ConditionOperator.LastMonth:
                case ConditionOperator.NextMonth:
                case ConditionOperator.LastWeek:
                case ConditionOperator.ThisWeek:
                case ConditionOperator.NextWeek:
                case ConditionOperator.InFiscalYear:
                    operatorExpression = TranslateConditionExpressionBetweenDates(c, getNonBasicValueExpr, containsAttributeExpression, context);
                    break;
#if FAKE_XRM_EASY_9
                case ConditionOperator.ContainValues:
                    operatorExpression = TranslateConditionExpressionContainValues(c, getNonBasicValueExpr, containsAttributeExpression);
                    break;

                case ConditionOperator.DoesNotContainValues:
                    operatorExpression = Expression.Not(TranslateConditionExpressionContainValues(c, getNonBasicValueExpr, containsAttributeExpression));
                    break;
#endif

                default:
                    throw new PullRequestException(string.Format("Operator {0} not yet implemented for condition expression", c.CondExpression.Operator.ToString()));


            }

            if (c.IsOuter)
            {
                //If outer join, filter is optional, only if there was a value
                return Expression.Constant(true);
            }
            else
                return operatorExpression;

        }

        

        

    }
}
