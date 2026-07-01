(function () {
    const form = document.getElementById('paymentGatewayForm');
    if (!form) return;

    const gatewayProfiles = {
        Paytm: {
            help: 'Paytm selected. Sirf Key ID aur Key Secret daalein — baaki sab auto set ho jayega.',
            displayName: 'Paytm Gateway',
            callbackPath: '/OnlinePayment/PaytmCallback',
            sandboxUrl: 'https://securegw-stage.paytm.in',
            productionUrl: 'https://securegw.paytm.in',
            industryType: 'Retail'
        },
        PhonePe: {
            help: 'PhonePe selected. Key ID aur Key Secret daalein — baaki sab auto set ho jayega.',
            displayName: 'PhonePe Gateway',
            callbackPath: '/OnlinePayment/PhonePeCallback',
            sandboxUrl: 'https://api-preprod.phonepe.com/apis/pg-sandbox',
            productionUrl: 'https://api.phonepe.com/apis/hermes',
            channelId: '1'
        },
        Razorpay: {
            help: 'Razorpay selected. Key ID aur Key Secret daalein — baaki sab auto set ho jayega.',
            displayName: 'Razorpay Gateway',
            callbackPath: '/OnlinePayment/RazorpayCallback',
            sandboxUrl: 'https://api.razorpay.com',
            productionUrl: 'https://api.razorpay.com'
        },
        Cashfree: {
            help: 'Cashfree selected. Key ID aur Key Secret daalein — baaki sab auto set ho jayega.',
            displayName: 'Cashfree Gateway',
            callbackPath: '/OnlinePayment/CashfreeCallback',
            sandboxUrl: 'https://sandbox.cashfree.com/pg',
            productionUrl: 'https://api.cashfree.com/pg'
        }
    };

    const validateBtn = document.getElementById('btnValidateApi');
    const saveBtn = document.getElementById('btnSaveGateway');
    const validationResult = document.getElementById('validationResult');
    const validationTokenInput = document.getElementById('ValidationToken');
    const isValidatedInput = document.getElementById('IsValidated');
    const tokenField = document.querySelector('input[name="__RequestVerificationToken"]');
    const gatewaySelect = document.getElementById('GatewayName');
    const environmentSelect = document.getElementById('Environment');
    const autoConfigPreview = document.getElementById('autoConfigPreview');

    function buildCallbackUrl(path) {
        return window.location.origin + path;
    }

    function applyGatewayProfile(clearCredentials) {
        const gateway = gatewaySelect?.value || 'Paytm';
        const profile = gatewayProfiles[gateway] || gatewayProfiles.Paytm;
        const environment = environmentSelect?.value || 'Sandbox';

        document.getElementById('gatewayHelp').textContent = profile.help;

        const displayName = document.getElementById('DisplayName');
        const merchantId = document.getElementById('MerchantId');
        const merchantKey = document.getElementById('MerchantKey');
        const callbackUrl = document.getElementById('CallbackUrl');
        const sandboxUrl = document.getElementById('SandboxBaseUrl');
        const productionUrl = document.getElementById('ProductionBaseUrl');
        const mid = document.getElementById('MID');
        const channelId = document.getElementById('ChannelId');
        const website = document.getElementById('Website');
        const industryType = document.getElementById('IndustryType');

        if (displayName) displayName.value = profile.displayName;
        if (callbackUrl) callbackUrl.value = buildCallbackUrl(profile.callbackPath);
        if (sandboxUrl) sandboxUrl.value = profile.sandboxUrl;
        if (productionUrl) productionUrl.value = profile.productionUrl;

        if (gateway === 'Paytm') {
            if (mid) mid.value = merchantId?.value || '';
            if (website) website.value = environment === 'Production' ? 'DEFAULT' : 'WEBSTAGING';
            if (industryType) industryType.value = profile.industryType || 'Retail';
            if (channelId) channelId.value = '';
        } else if (gateway === 'PhonePe') {
            if (channelId) channelId.value = profile.channelId || '1';
            if (mid) mid.value = '';
            if (website) website.value = '';
            if (industryType) industryType.value = '';
        } else {
            if (mid) mid.value = '';
            if (channelId) channelId.value = '';
            if (website) website.value = '';
            if (industryType) industryType.value = '';
        }

        if (clearCredentials) {
            if (merchantId) merchantId.value = '';
            if (merchantKey) merchantKey.value = '';
            resetValidationState();
        } else if (gateway === 'Paytm' && mid && merchantId?.value) {
            mid.value = merchantId.value;
        }

        if (autoConfigPreview) {
            autoConfigPreview.innerHTML =
                '<strong>Auto configured:</strong> ' +
                profile.displayName + ' · ' +
                environment + ' · Callback: <code>' + (callbackUrl?.value || '') + '</code>';
        }
    }

    function resetValidationState() {
        if (saveBtn) saveBtn.disabled = true;
        if (validationTokenInput) validationTokenInput.value = '';
        if (isValidatedInput) isValidatedInput.value = 'false';
        if (validationResult) {
            validationResult.className = 'alert d-none mb-3';
            validationResult.textContent = '';
        }
    }

    document.querySelectorAll('.pg-field').forEach(function (el) {
        el.addEventListener('change', resetValidationState);
        el.addEventListener('input', resetValidationState);
    });

    document.getElementById('MerchantId')?.addEventListener('input', function () {
        if (gatewaySelect?.value === 'Paytm') {
            const mid = document.getElementById('MID');
            if (mid) mid.value = this.value;
        }
    });

    gatewaySelect?.addEventListener('change', function () {
        applyGatewayProfile(true);
    });

    environmentSelect?.addEventListener('change', function () {
        applyGatewayProfile(false);
        resetValidationState();
    });

    applyGatewayProfile(false);

    saveBtn?.addEventListener('click', function () {
        applyGatewayProfile(false);
    });

    form.addEventListener('submit', function () {
        applyGatewayProfile(false);
    });

    if (!validateBtn) return;

    validateBtn.addEventListener('click', async function () {
        applyGatewayProfile(false);

        const spinner = validateBtn.querySelector('.validate-spinner');
        const text = validateBtn.querySelector('.validate-text');

        validateBtn.disabled = true;
        spinner?.classList.remove('d-none');
        text?.classList.add('d-none');

        try {
            const payload = {
                id: parseInt(document.getElementById('Id')?.value || '0', 10),
                gatewayName: document.getElementById('GatewayName')?.value,
                displayName: document.getElementById('DisplayName')?.value,
                merchantId: document.getElementById('MerchantId')?.value,
                merchantKey: document.getElementById('MerchantKey')?.value,
                mid: document.getElementById('MID')?.value,
                channelId: document.getElementById('ChannelId')?.value,
                website: document.getElementById('Website')?.value,
                industryType: document.getElementById('IndustryType')?.value,
                callbackUrl: document.getElementById('CallbackUrl')?.value,
                environment: document.getElementById('Environment')?.value,
                sandboxBaseUrl: document.getElementById('SandboxBaseUrl')?.value,
                productionBaseUrl: document.getElementById('ProductionBaseUrl')?.value,
                isActive: document.getElementById('IsActive')?.checked ?? true,
                isDefault: document.getElementById('IsDefault')?.checked ?? false,
                hasExistingKey: document.getElementById('HasExistingKey')?.value === 'True'
            };

            const response = await fetch('/PaymentGateway/ValidateApi', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': tokenField?.value || ''
                },
                body: JSON.stringify(payload)
            });

            const result = await response.json();
            validationResult?.classList.remove('d-none');

            if (result.success) {
                if (validationResult) {
                    validationResult.className = 'alert alert-success mb-3';
                    validationResult.innerHTML = '<i class="fa-solid fa-circle-check me-2"></i>' + (result.message || 'Credentials verified');
                }
                if (validationTokenInput) validationTokenInput.value = result.validationToken || '';
                if (isValidatedInput) isValidatedInput.value = 'true';
                if (saveBtn) saveBtn.disabled = false;
                if (window.toastr) toastr.success(result.message || 'Credentials verified');
            } else {
                if (validationResult) {
                    validationResult.className = 'alert alert-danger mb-3';
                    validationResult.textContent = result.message || 'Validation failed.';
                }
                if (saveBtn) saveBtn.disabled = true;
                if (window.toastr) toastr.error(result.message || 'Validation failed.');
            }
        } catch {
            if (validationResult) {
                validationResult.className = 'alert alert-danger mb-3';
                validationResult.textContent = 'Unable to validate credentials. Please try again.';
            }
            if (saveBtn) saveBtn.disabled = true;
        } finally {
            validateBtn.disabled = false;
            spinner?.classList.add('d-none');
            text?.classList.remove('d-none');
        }
    });
})();
