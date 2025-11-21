// wwwroot/js/site.js

// Utility functions
const utils = {
    // Debounce function for search inputs
    debounce(func, wait, immediate) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                timeout = null;
                if (!immediate) func(...args);
            };
            const callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow) func(...args);
        };
    },

    // Format currency
    formatCurrency(amount) {
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD'
        }).format(amount);
    },

    // Format date
    formatDate(date) {
        return new Intl.DateTimeFormat('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        }).format(new Date(date));
    },

    // Show notification
    showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `fixed top-4 right-4 z-50 p-4 rounded-xl shadow-lg border transform translate-x-full transition-transform duration-300 ${type === 'success' ? 'bg-green-50 border-green-200 text-green-800' :
                type === 'error' ? 'bg-red-50 border-red-200 text-red-800' :
                    type === 'warning' ? 'bg-yellow-50 border-yellow-200 text-yellow-800' :
                        'bg-blue-50 border-blue-200 text-blue-800'
            }`;

        notification.innerHTML = `
            <div class="flex items-center space-x-3">
                <i class="fas ${type === 'success' ? 'fa-check-circle' :
                type === 'error' ? 'fa-exclamation-circle' :
                    type === 'warning' ? 'fa-exclamation-triangle' :
                        'fa-info-circle'
            }"></i>
                <span>${message}</span>
            </div>
        `;

        document.body.appendChild(notification);

        // Animate in
        setTimeout(() => {
            notification.style.transform = 'translateX(0)';
        }, 100);

        // Auto remove after 5 seconds
        setTimeout(() => {
            notification.style.transform = 'translateX(100%)';
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.parentNode.removeChild(notification);
                }
            }, 300);
        }, 5000);
    },

    // Confirm dialog
    confirm(message) {
        return new Promise((resolve) => {
            const overlay = document.createElement('div');
            overlay.className = 'fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4';

            overlay.innerHTML = `
                <div class="bg-white rounded-2xl p-6 max-w-sm w-full mx-auto scale-95 opacity-0 transition-all duration-300">
                    <div class="text-center">
                        <div class="w-12 h-12 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-4">
                            <i class="fas fa-exclamation-triangle text-red-600 text-lg"></i>
                        </div>
                        <h3 class="text-lg font-semibold text-gray-900 mb-2">Confirm Action</h3>
                        <p class="text-gray-600 mb-6">${message}</p>
                        <div class="flex space-x-3">
                            <button class="btn btn-secondary flex-1" data-action="cancel">Cancel</button>
                            <button class="btn bg-red-600 text-white hover:bg-red-700 flex-1" data-action="confirm">Confirm</button>
                        </div>
                    </div>
                </div>
            `;

            document.body.appendChild(overlay);

            // Animate in
            setTimeout(() => {
                overlay.querySelector('.scale-95').classList.remove('scale-95', 'opacity-0');
            }, 100);

            // Handle button clicks
            overlay.querySelectorAll('button').forEach(button => {
                button.addEventListener('click', (e) => {
                    const action = e.target.getAttribute('data-action');

                    // Animate out
                    overlay.querySelector('.scale-95').classList.add('scale-95', 'opacity-0');

                    setTimeout(() => {
                        document.body.removeChild(overlay);
                        resolve(action === 'confirm');
                    }, 300);
                });
            });
        });
    }
};

// Auto-calculation for claim submission
function initializeClaimCalculator() {
    const hoursInput = document.getElementById('HoursWorked');
    const rateInput = document.getElementById('HourlyRate');
    const totalDisplay = document.getElementById('TotalAmountDisplay');
    const totalHidden = document.getElementById('TotalAmount');

    if (hoursInput && rateInput && totalDisplay) {
        function calculateTotal() {
            const hours = parseFloat(hoursInput.value) || 0;
            const rate = parseFloat(rateInput.value) || 0;
            const total = hours * rate;

            totalDisplay.textContent = `R ${total.toFixed(2)}`;
            if (totalHidden) {
                totalHidden.value = total.toFixed(2);
            }

            // Validation feedback
            validateHours(hours);
            validateRate(rate);
            validateTotalAmount(total);
        }

        function validateHours(hours) {
            const feedback = document.getElementById('hoursFeedback');
            const inputGroup = hoursInput.closest('.form-group');

            if (feedback) {
                if (hours <= 0) {
                    feedback.textContent = 'Hours must be greater than 0';
                    feedback.className = 'text-red-600 text-sm mt-1';
                    inputGroup?.classList.add('error');
                } else if (hours > 100) {
                    feedback.textContent = 'Hours cannot exceed 100';
                    feedback.className = 'text-red-600 text-sm mt-1';
                    inputGroup?.classList.add('error');
                } else {
                    feedback.textContent = '';
                    inputGroup?.classList.remove('error');
                    inputGroup?.classList.add('success');
                }
            }
        }

        function validateRate(rate) {
            const feedback = document.getElementById('rateFeedback');
            const inputGroup = rateInput.closest('.form-group');

            if (feedback) {
                if (rate <= 0) {
                    feedback.textContent = 'Hourly rate must be greater than 0';
                    feedback.className = 'text-red-600 text-sm mt-1';
                    inputGroup?.classList.add('error');
                } else if (rate > 500) {
                    feedback.textContent = 'Hourly rate cannot exceed R500';
                    feedback.className = 'text-red-600 text-sm mt-1';
                    inputGroup?.classList.add('error');
                } else {
                    feedback.textContent = '';
                    inputGroup?.classList.remove('error');
                    inputGroup?.classList.add('success');
                }
            }
        }

        function validateTotalAmount(total) {
            const feedback = document.getElementById('totalFeedback');
            if (feedback) {
                if (total > 50000) {
                    feedback.textContent = 'Total amount cannot exceed R50,000';
                    feedback.className = 'text-red-600 text-sm mt-1';
                } else {
                    feedback.textContent = '';
                }
            }
        }

        hoursInput.addEventListener('input', calculateTotal);
        rateInput.addEventListener('input', calculateTotal);

        // Initial calculation
        calculateTotal();
    }
}

// Real-time status tracking with SignalR
function initializeClaimTracking(claimId, trackingId) {
    if (typeof signalR === 'undefined') {
        console.warn('SignalR not loaded. Real-time features disabled.');
        return;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/claimManagementHub")
        .withAutomaticReconnect()
        .build();

    connection.start()
        .then(() => {
            console.log("Connected to SignalR hub for claim tracking");
            return connection.invoke("JoinClaimGroup", trackingId);
        })
        .then(() => {
            console.log(`Joined claim group: ${trackingId}`);
        })
        .catch(err => console.error("SignalR connection error:", err));

    connection.on("StatusUpdated", (update) => {
        console.log('Status update received:', update);
        utils.showNotification(`Claim status updated: ${update.Status}`, 'info');
        updateStatusDisplay(update.Status, update.ProgressStatus);

        // Update progress bar if exists
        updateProgressBar(update.ProgressStatus);
    });

    connection.on("ClaimApproved", (approval) => {
        console.log('Claim approved:', approval);
        utils.showNotification(`Your claim has been approved by ${approval.ApprovedBy}!`, 'success');
        updateStatusDisplay('approved', 'Approved');
        updateProgressBar('Approved');
    });

    connection.on("ClaimRejected", (rejection) => {
        console.log('Claim rejected:', rejection);
        utils.showNotification(`Your claim was rejected. Reason: ${rejection.Reason}`, 'error');
        updateStatusDisplay('rejected', 'Rejected');
        updateProgressBar('Rejected');
    });

    connection.onreconnecting(() => {
        console.log('SignalR reconnecting...');
        utils.showNotification('Reconnecting to claim tracking...', 'warning');
    });

    connection.onreconnected(() => {
        console.log('SignalR reconnected');
        utils.showNotification('Claim tracking reconnected', 'success');
    });
}

function updateStatusDisplay(status, progressStatus) {
    const statusElement = document.getElementById('claimStatus');
    const progressElement = document.getElementById('progressStatus');

    if (statusElement) {
        statusElement.textContent = status.charAt(0).toUpperCase() + status.slice(1);

        // Update status badge classes
        statusElement.className = 'inline-flex items-center px-3 py-1 rounded-full text-sm font-medium';
        if (status === 'approved' || status === 'auto-approved') {
            statusElement.classList.add('bg-green-100', 'text-green-800');
        } else if (status === 'rejected') {
            statusElement.classList.add('bg-red-100', 'text-red-800');
        } else if (status === 'pending') {
            statusElement.classList.add('bg-yellow-100', 'text-yellow-800');
        } else {
            statusElement.classList.add('bg-gray-100', 'text-gray-800');
        }
    }

    if (progressElement) {
        progressElement.textContent = progressStatus;
    }
}

function updateProgressBar(progressStatus) {
    const progressBar = document.getElementById('progressBar');
    const progressSteps = document.querySelectorAll('.progress-step');

    if (!progressBar) return;

    const statusToPercent = {
        'Submitted': 25,
        'Under Review': 50,
        'Auto-Approved': 100,
        'Approved': 100,
        'Rejected': 100
    };

    const percent = statusToPercent[progressStatus] || 0;
    progressBar.style.width = `${percent}%`;

    // Update step indicators
    progressSteps.forEach(step => {
        const stepStatus = step.getAttribute('data-status');
        if (stepStatus === progressStatus) {
            step.classList.add('active');
        } else {
            step.classList.remove('active');
        }
    });
}

// Enhanced file upload with preview
function initializeFileUpload() {
    const fileInput = document.getElementById('SupportingDocument');
    const fileNameDisplay = document.getElementById('fileNameDisplay');
    const filePreview = document.getElementById('filePreview');
    const removeFileBtn = document.getElementById('removeFile');
    const fileSizeDisplay = document.getElementById('fileSizeDisplay');

    if (fileInput && fileNameDisplay) {
        fileInput.addEventListener('change', function (e) {
            const file = e.target.files[0];
            if (file) {
                fileNameDisplay.textContent = file.name;

                // Display file size
                if (fileSizeDisplay) {
                    const fileSize = (file.size / (1024 * 1024)).toFixed(2);
                    fileSizeDisplay.textContent = `${fileSize} MB`;
                }

                // Validate file size
                if (file.size > 5 * 1024 * 1024) { // 5MB limit
                    utils.showNotification('File size must be less than 5MB', 'error');
                    fileInput.value = '';
                    fileNameDisplay.textContent = 'No file chosen';
                    if (fileSizeDisplay) fileSizeDisplay.textContent = '';
                    return;
                }

                // Validate file type
                const allowedTypes = ['.pdf', '.docx', '.xlsx', '.jpg', '.jpeg', '.png'];
                const fileExtension = '.' + file.name.split('.').pop().toLowerCase();
                if (!allowedTypes.includes(fileExtension)) {
                    utils.showNotification('Invalid file type. Allowed: PDF, DOCX, XLSX, JPG, JPEG, PNG', 'error');
                    fileInput.value = '';
                    fileNameDisplay.textContent = 'No file chosen';
                    if (fileSizeDisplay) fileSizeDisplay.textContent = '';
                    return;
                }

                // Show preview for images
                if (file.type.startsWith('image/')) {
                    const reader = new FileReader();
                    reader.onload = function (e) {
                        if (filePreview) {
                            filePreview.innerHTML = `
                                <div class="relative">
                                    <img src="${e.target.result}" class="max-w-xs rounded-lg shadow-md" alt="Preview">
                                    <button type="button" onclick="removeFilePreview()" class="absolute -top-2 -right-2 bg-red-500 text-white rounded-full w-6 h-6 flex items-center justify-center text-xs hover:bg-red-600">
                                        <i class="fas fa-times"></i>
                                    </button>
                                </div>
                            `;
                            filePreview.classList.remove('hidden');
                        }
                    };
                    reader.readAsDataURL(file);
                } else {
                    if (filePreview) {
                        filePreview.innerHTML = `
                            <div class="flex items-center space-x-3 p-4 bg-gray-50 rounded-lg">
                                <i class="fas fa-file text-2xl text-blue-500"></i>
                                <div>
                                    <div class="font-medium">${file.name}</div>
                                    <div class="text-sm text-gray-500">${(file.size / 1024).toFixed(0)} KB</div>
                                </div>
                                <button type="button" onclick="removeFilePreview()" class="ml-auto text-red-500 hover:text-red-700">
                                    <i class="fas fa-times"></i>
                                </button>
                            </div>
                        `;
                        filePreview.classList.remove('hidden');
                    }
                }

                if (removeFileBtn) {
                    removeFileBtn.classList.remove('hidden');
                }
            }
        });
    }

    if (removeFileBtn) {
        removeFileBtn.addEventListener('click', function () {
            fileInput.value = '';
            fileNameDisplay.textContent = 'No file chosen';
            if (filePreview) filePreview.classList.add('hidden');
            if (fileSizeDisplay) fileSizeDisplay.textContent = '';
            removeFileBtn.classList.add('hidden');
        });
    }
}

// Global function to remove file preview
function removeFilePreview() {
    const fileInput = document.getElementById('SupportingDocument');
    const fileNameDisplay = document.getElementById('fileNameDisplay');
    const filePreview = document.getElementById('filePreview');
    const fileSizeDisplay = document.getElementById('fileSizeDisplay');
    const removeFileBtn = document.getElementById('removeFile');

    if (fileInput) fileInput.value = '';
    if (fileNameDisplay) fileNameDisplay.textContent = 'No file chosen';
    if (filePreview) filePreview.classList.add('hidden');
    if (fileSizeDisplay) fileSizeDisplay.textContent = '';
    if (removeFileBtn) removeFileBtn.classList.add('hidden');
}

// Form handling
const formHandler = {
    init() {
        // Add loading states to forms
        document.addEventListener('submit', (e) => {
            const form = e.target;
            const submitButton = form.querySelector('button[type="submit"]');

            if (submitButton) {
                submitButton.classList.add('loading');
                submitButton.disabled = true;

                // Re-enable button if form submission fails
                setTimeout(() => {
                    submitButton.classList.remove('loading');
                    submitButton.disabled = false;
                }, 5000);
            }
        });

        // Add real-time validation
        document.addEventListener('input', (e) => {
            if (e.target.matches('[data-validate]')) {
                this.validateField(e.target);
            }
        });
    },

    validateField(field) {
        const value = field.value.trim();
        let isValid = true;
        let errorMessage = '';

        // Required validation
        if (field.hasAttribute('required') && !value) {
            isValid = false;
            errorMessage = 'This field is required';
        }

        // Email validation
        if (field.type === 'email' && value) {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(value)) {
                isValid = false;
                errorMessage = 'Please enter a valid email address';
            }
        }

        // Number validation
        if (field.type === 'number' && value) {
            if (isNaN(value)) {
                isValid = false;
                errorMessage = 'Please enter a valid number';
            }
        }

        // Update field state
        this.setFieldState(field, isValid, errorMessage);
        return isValid;
    },

    setFieldState(field, isValid, errorMessage) {
        const errorElement = field.parentNode.querySelector('.error-message');

        if (isValid) {
            field.classList.remove('error');
            field.classList.add('success');
            if (errorElement) {
                errorElement.remove();
            }
        } else {
            field.classList.add('error');
            field.classList.remove('success');

            if (!errorElement) {
                const errorDiv = document.createElement('div');
                errorDiv.className = 'error-message text-red-600 text-xs mt-1';
                errorDiv.textContent = errorMessage;
                field.parentNode.appendChild(errorDiv);
            } else {
                errorElement.textContent = errorMessage;
            }
        }
    }
};

// Initialize all features when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    // Initialize form handling
    formHandler.init();

    // Initialize claim calculator
    initializeClaimCalculator();

    // Initialize file upload
    initializeFileUpload();

    // Add smooth scrolling to anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    // Add loading states to buttons
    document.addEventListener('click', (e) => {
        if (e.target.matches('[data-loading]')) {
            const button = e.target;
            const originalText = button.innerHTML;

            button.innerHTML = `
                <div class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
                Loading...
            `;
            button.disabled = true;

            // Revert after 5 seconds if still loading
            setTimeout(() => {
                button.innerHTML = originalText;
                button.disabled = false;
            }, 5000);
        }
    });

    // Auto-dismiss alerts
    const autoDismissAlerts = document.querySelectorAll('[data-auto-dismiss]');
    autoDismissAlerts.forEach(alert => {
        const delay = parseInt(alert.getAttribute('data-auto-dismiss')) || 5000;
        setTimeout(() => {
            alert.style.opacity = '0';
            setTimeout(() => {
                if (alert.parentNode) {
                    alert.parentNode.removeChild(alert);
                }
            }, 300);
        }, delay);
    });

    // Check if we're on a claim tracking page
    const claimId = document.getElementById('claimId')?.value;
    const trackingId = document.getElementById('trackingId')?.value;
    if (claimId && trackingId) {
        initializeClaimTracking(claimId, trackingId);
    }

    // Initialize real-time features for coordinators
    if (document.body.classList.contains('coordinator-dashboard')) {
        initializeCoordinatorRealTime();
    }
});

// Coordinator real-time features
function initializeCoordinatorRealTime() {
    if (typeof signalR === 'undefined') {
        console.warn('SignalR not loaded. Real-time features disabled.');
        return;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/claimManagementHub")
        .withAutomaticReconnect()
        .build();

    connection.start()
        .then(() => {
            console.log("Connected to SignalR hub for coordinator dashboard");
            return connection.invoke("JoinGroup", "Coordinators");
        })
        .catch(err => console.error("SignalR connection error:", err));

    connection.on("NewClaimSubmitted", (claim) => {
        console.log('New claim submitted:', claim);
        utils.showNotification(`New claim submitted by ${claim.LecturerName} for R${claim.Amount}`, 'info');

        // Refresh claims list if on verify claims page
        if (window.location.pathname.includes('/Coordinator/VerifyClaims')) {
            setTimeout(() => {
                window.location.reload();
            }, 2000);
        }
    });

    connection.on("ClaimStatusChanged", (update) => {
        console.log('Claim status changed:', update);
        // Update specific claim in the list if needed
        const claimRow = document.querySelector(`[data-claim-id="${update.ClaimId}"]`);
        if (claimRow) {
            const statusCell = claimRow.querySelector('.claim-status');
            if (statusCell) {
                statusCell.textContent = update.Status;
                statusCell.className = `claim-status ${getStatusBadgeClass(update.Status)}`;
            }
        }
    });
}

function getStatusBadgeClass(status) {
    const baseClasses = 'inline-flex items-center px-2 py-1 rounded-full text-xs font-medium';
    switch (status) {
        case 'approved':
        case 'auto-approved':
            return `${baseClasses} bg-green-100 text-green-800`;
        case 'rejected':
            return `${baseClasses} bg-red-100 text-red-800`;
        case 'pending':
            return `${baseClasses} bg-yellow-100 text-yellow-800`;
        default:
            return `${baseClasses} bg-gray-100 text-gray-800`;
    }
}

// Make utils available globally
window.utils = utils;
window.formHandler = formHandler;
window.removeFilePreview = removeFilePreview;
window.initializeClaimTracking = initializeClaimTracking;