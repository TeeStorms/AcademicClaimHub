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

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    // Initialize form handling
    formHandler.init();

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
});

// Make utils available globally
window.utils = utils;
window.formHandler = formHandler;