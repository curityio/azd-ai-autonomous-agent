/**
 * This is an example procedure that performs no transformation
 * @param {se.curity.identityserver.procedures.context.TransformationProcedureContext} context
 * @returns {*}
 */
function result(context) {
  var attributes = context.attributeMap;

  if (attributes.location && attributes.employee_id) {

    // Apply your preferred logic to transform received user attributes from external identity
    attributes.region = attributes.location;
    attributes.customer_id = attributes.employee_id;
  } else {

    // For demo purposes, the example deployment sets hard coded values if received attributes are missing
    // 
    attributes.region = "USA"
    attributes.customer_id = "178"
    logger.error("*************** DEBUG ***")
    logger.error(Math.floor(Math.random() * 100))
    logger.error("*************** DEBUG ***")
  }

  return attributes;
}
