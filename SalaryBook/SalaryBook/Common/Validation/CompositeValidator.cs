﻿using Pellared.Utils.Mvvm.Validation.Generic;

namespace Pellared.Utils.Mvvm.Validation
{
    public class CompositeValidator<TObject> : CompositeValidator<TObject, ValidationError>, IValidator<TObject>
    {
        public CompositeValidator(params IValidator<TObject, ValidationError>[] validators)
            : base(validators)
        {
            
        }
    }
}