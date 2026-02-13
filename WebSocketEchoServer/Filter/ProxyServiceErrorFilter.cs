using AetherCore.Utility.Exceptions;
using AetherCore.Exceptions;

namespace OpenAIProxyService.Controllers
{
    public class ProxyServiceErrorCode
    {
        // auth
        // admin auth
        // chat role
        // conversation
        // message
        // knowledge guide
        // user       
    }

    public class ProxyServiceErrorFilter : GlobalExceptionFilter
    {
        protected override CustomErrorResponse FilterOtherError(CustomException customException)
        {
            var response = new CustomErrorResponse();

            switch (customException)
            {               

                //    case DishItemDatabaseOperationException exDishDb:
                //        response.StatusCode = StatusCodes.Status500InternalServerError;
                //        response.ErrorCode  = POSServiceErrorCode.DishItemDatabaseOperation;
                //        response.Message    = "Dish item database operation failed.";
                //        response.Detail     = exDishDb.Message;
                //        return response;

                default:
                    // 沒有命中，讓 GlobalExceptionFilter 處理
                    return base.FilterOtherError(customException);
            }
        }
    }
}
