using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Definit.Common.Server.OData
{



    /// <summary>
    /// Based on  odata.net/src/Microsoft.OData.Core/UriParser/Resolver/StringAsEnumResolver.cs
    /// Modified to support numeric/non-string values for Enums
    /// Also allows inheritor to add conversions for other datatypes by overriding PromoteBinaryOperandTypes 
    /// and call ConvertValue for each conversion it supports.
    /// </summary>
    public class ConvertingODataUriResolver : ODataUriResolver
    {
        public override void PromoteBinaryOperandTypes(
               BinaryOperatorKind binaryOperatorKind,
               ref SingleValueNode leftNode,
               ref SingleValueNode rightNode,
               out IEdmTypeReference typeReference)
        {
            typeReference = null;

            if (ConvertValue(ref leftNode, ref rightNode, t => t.IsEnum(), ConvertEnumNode))
                return;

            base.PromoteBinaryOperandTypes(binaryOperatorKind, ref leftNode, ref rightNode, out typeReference);
        }

        /// <summary>
        /// Resolve operation's parameters. Using this extension, enum value could be written as string value.
        /// </summary>
        /// <param name="operation">Current operation for parameters.</param>
        /// <param name="input">A dictionary the parameter list.</param>
        /// <returns>A dictionary containing resolved parameters.</returns>
        public override IDictionary<IEdmOperationParameter, SingleValueNode> ResolveOperationParameters(IEdmOperation operation, IDictionary<string, SingleValueNode> input)
        {
            Dictionary<IEdmOperationParameter, SingleValueNode> result = new Dictionary<IEdmOperationParameter, SingleValueNode>(EqualityComparer<IEdmOperationParameter>.Default);
            foreach (var item in input)
            {
                IEdmOperationParameter functionParameter = null;
                if (EnableCaseInsensitive)
                {
                    functionParameter = operation.FindParameter(item.Key); //ODataUriResolver.ResolveOperationParameterNameCaseInsensitive(operation, item.Key);
                }
                else
                {
                    functionParameter = operation.FindParameter(item.Key);
                }

                // ensure parameter name existis
                if (functionParameter == null)
                {
                    throw new ODataException("");
                }

                SingleValueNode newVal = item.Value;

                if (functionParameter.Type.IsEnum()
                    && newVal is ConstantNode
                    && newVal.TypeReference != null
                    && newVal.TypeReference.IsString())
                {
                    string text = ((ConstantNode)item.Value).Value as string;
                    ODataEnumValue val;
                    IEdmTypeReference typeRef = functionParameter.Type;

                    if (TryParseEnum(typeRef.Definition as IEdmEnumType, text, out val))
                    {
                        newVal = new ConstantNode(val, text, typeRef);
                    }
                }

                result.Add(functionParameter, newVal);
            }

            return result;
        }

        /// <summary>
        /// Resolve keys for certain entity set, this function would be called when key is specified as positional values. E.g. EntitySet('key')
        /// Enum value could omit type name prefix using this resolver.
        /// </summary>
        /// <param name="type">Type for current entityset.</param>
        /// <param name="positionalValues">The list of positional values.</param>
        /// <param name="convertFunc">The convert function to be used for value converting.</param>
        /// <returns>The resolved key list.</returns>
        public override IEnumerable<KeyValuePair<string, object>> ResolveKeys(IEdmEntityType type, IList<string> positionalValues, Func<IEdmTypeReference, string, object> convertFunc)
        {
            return base.ResolveKeys(
                type,
                positionalValues,
                (typeRef, valueText) =>
                {
                    if (typeRef.IsEnum() && valueText.StartsWith("'", StringComparison.Ordinal) && valueText.EndsWith("'", StringComparison.Ordinal))
                    {
                        valueText = typeRef.FullName() + valueText;
                    }

                    return convertFunc(typeRef, valueText);
                });
        }

        /// <summary>
        /// Resolve keys for certain entity set, this function would be called when key is specified as name value pairs. E.g. EntitySet(ID='key')
        /// Enum value could omit type name prefix using this resolver.
        /// </summary>
        /// <param name="type">Type for current entityset.</param>
        /// <param name="namedValues">The dictionary of name value pairs.</param>
        /// <param name="convertFunc">The convert function to be used for value converting.</param>
        /// <returns>The resolved key list.</returns>
        public override IEnumerable<KeyValuePair<string, object>> ResolveKeys(IEdmEntityType type, IDictionary<string, string> namedValues, Func<IEdmTypeReference, string, object> convertFunc)
        {
            return base.ResolveKeys(
                type,
                namedValues,
                (typeRef, valueText) =>
                {
                    if (typeRef.IsEnum() && valueText.StartsWith("'", StringComparison.Ordinal) && valueText.EndsWith("'", StringComparison.Ordinal))
                    {
                        valueText = typeRef.FullName() + valueText;
                    }

                    return convertFunc(typeRef, valueText);
                });
        }

        /// <summary>
        /// Returns true if conversion has been performed
        /// </summary>
        /// <param name="leftNode"></param>
        /// <param name="rightNode"></param>
        /// <param name="typeCriteria"></param>
        /// <param name="convertFunc"></param>
        /// <returns></returns>
        protected static bool ConvertValue(ref SingleValueNode leftNode, ref SingleValueNode rightNode, Func<IEdmTypeReference, bool> typeCriteria, Func<SingleValueNode, ConstantNode, ConstantNode> convertFunc)
        {
            if (leftNode.TypeReference == null || rightNode.TypeReference == null)
                return false;
                        
            if (typeCriteria(leftNode.TypeReference) && rightNode is ConstantNode constantNode)
            {
                rightNode = convertFunc(leftNode, constantNode);
                return true;
            }
            else if (typeCriteria(rightNode.TypeReference) && leftNode is ConstantNode)
            {
                leftNode = convertFunc(rightNode, leftNode as ConstantNode);
                return true;
            }
            return false;
        }

        private static ConstantNode ConvertEnumNode(SingleValueNode propertyNode, ConstantNode constantNode)
        {
            string text = constantNode.Value.ToString();
            IEdmTypeReference typeRef = propertyNode.TypeReference;

            if (TryParseEnum(typeRef.Definition as IEdmEnumType, text, out var parsedValue))
            {
                return new ConstantNode(parsedValue, text, typeRef);
            }

            return constantNode;
        }

        /// <summary>
        /// Parse string or integer to enum value
        /// </summary>
        /// <param name="enumType">edm enum type</param>
        /// <param name="value">input string value</param>
        /// <param name="enumValue">output edm enum value</param>
        /// <returns>true if parse succeeds, false if fails</returns>
        private static bool TryParseEnum(IEdmEnumType enumType, string value, out ODataEnumValue enumValue)
        {
            bool success = enumType.TryParseEnum(value, true, out var parsedValue);
            if (success)
            {
                enumValue = new ODataEnumValue(parsedValue.ToString(CultureInfo.InvariantCulture), enumType.FullTypeName());
            }
            else
            {
                enumValue = null;
            }

            return success;
        }
    }
}
