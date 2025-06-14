﻿using HealthyNutritionApp.Application.Dto.Order;

namespace HealthyNutritionApp.Application.Dto.Payment
{
    public record CreatePaymentLinkRequest
    (
        OrderInformationRequest OrderInformation,
        string ReturnUrl,
        string CancelUrl
    );
}
