using System.Web.Mvc;

namespace DayKast_VFeb2026.Helpers
{
    /// <summary>
    /// Tüm string model property'lerini otomatik olarak Trim() eder.
    /// Baştan ve sondan boşlukları temizler.
    /// </summary>
    public class TrimModelBinder : DefaultModelBinder
    {
        protected override void SetProperty(ControllerContext controllerContext,
            ModelBindingContext bindingContext, System.ComponentModel.PropertyDescriptor propertyDescriptor, object value)
        {
            if (propertyDescriptor.PropertyType == typeof(string) && value is string strValue)
            {
                value = strValue.Trim();
            }

            base.SetProperty(controllerContext, bindingContext, propertyDescriptor, value);
        }
    }
}
