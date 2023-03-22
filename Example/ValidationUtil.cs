using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Sharara.Services.Kumusha
{
    static class ValidationUtil {

        public static IEnumerable<ValidationResult> ValidateString(
            [CallerMemberName] string propertyName = null) {

            ArgumentNullException.ThrowIfNull(propertyName);
            var result = new List<ValidationResult>();
            return result;

        }

    }
}