let currentStep = 1;

function nextStep(step) {
    const current = document.getElementById(`step${step}`);
    const next = document.getElementById(`step${step + 1}`);

    if (validateStep(current, step)) {
        current.classList.add('d-none');
        next.classList.remove('d-none');
    }
}

function prevStep(step) {
    document.getElementById(`step${step + 1}`).classList.add('d-none');
    document.getElementById(`step${step}`).classList.remove('d-none');
}

function validateStep(stepDiv, stepNumber) {
    let inputs = stepDiv.querySelectorAll('input, textarea');
    let isValid = true;

    // Remove previous errors
    stepDiv.querySelectorAll('.text-danger').forEach(e => e.remove());
    stepDiv.querySelectorAll('.is-invalid').forEach(e => e.classList.remove('is-invalid'));

    for (let input of inputs) {
        // Required + pattern validation
        if (!input.checkValidity()) {
            input.classList.add('is-invalid');
            input.reportValidity(); // show browser tooltip too
            isValid = false;
        }

        // File size validation for file inputs (<= 1MB)
        if (input.type === "file" && input.files.length > 0) {
            let file = input.files[0];
            if (file.size > 1 * 1024 * 1024) {
                input.classList.add('is-invalid');

                let errorMsg = document.createElement('div');
                errorMsg.className = 'text-danger mt-1';
                errorMsg.innerText = 'File must be less than 1 MB.';
                input.parentElement.appendChild(errorMsg);

                isValid = false;
            }
        }
    }

    // Password match validation (step 2 only)
    if (stepNumber === 2) {
        let password = stepDiv.querySelector('input[name="Password"]');
        let confirmPassword = stepDiv.querySelector('input[name="ConfirmPassword"]');
        if (password && confirmPassword && password.value !== confirmPassword.value) {
            confirmPassword.classList.add('is-invalid');

            let errorMsg = document.createElement('div');
            errorMsg.className = 'text-danger mt-1';
            errorMsg.innerText = 'Passwords do not match.';
            confirmPassword.parentElement.appendChild(errorMsg);

            isValid = false;
        }
    }

    return isValid;
}

// Final Submit with SweetAlert
document.getElementById('regForm').addEventListener('submit', function (e) {
    const step3 = document.getElementById('step3');

    if (!validateStep(step3, 3)) {
        e.preventDefault(); // Only stop submission if validation fails
        return false;
    }

});