/**
 * This is an example procedure that performs no transformation
 * @param {se.curity.identityserver.procedures.context.TransformationProcedureContext} context
 * @returns {*}
 */
function result(context) {
  var attributes = context.attributeMap;

  if (attributes.location && attributes.employee_id) {

    // Apply your preferred logic to transform Entra ID user attributes
    attributes.region = attributes.location;
    attributes.customer_id = attributes.employee_id;
  } else {

    // The example sets some hard coded properties if Entra ID attributes are missing
    attributes.region = "USA"
    attributes.customer_id = "2109"
  }

  return attributes;
}
