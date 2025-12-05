// Counter Animation for Stats Section
document.addEventListener('DOMContentLoaded', function() {
    const counters = document.querySelectorAll('.counter');
    const statsSection = document.querySelector('#stats-section');
    let hasAnimated = false;

    // Function to animate counter
    function animateCounter(counter) {
        const target = parseFloat(counter.getAttribute('data-target'));
        const suffix = counter.getAttribute('data-suffix') || '';
        const duration = 2000; // 2 seconds
        const step = target / (duration / 16); // 60fps
        let current = 0;

        const updateCounter = () => {
            current += step;
            
            if (current >= target) {
                // Handle different number formats
                if (target >= 1000000) {
                    counter.textContent = '1M' + suffix;
                } else if (target >= 1000) {
                    counter.textContent = Math.floor(target / 1000) + 'K' + suffix;
                } else if (target % 1 !== 0) {
                    // Decimal number
                    counter.textContent = target.toFixed(1) + suffix;
                } else {
                    counter.textContent = target + suffix;
                }
                return;
            }

            // Format current value
            let displayValue;
            if (target >= 1000000) {
                displayValue = (current / 1000000).toFixed(1) + 'M';
            } else if (target >= 1000) {
                displayValue = Math.floor(current / 1000) + 'K';
            } else if (target % 1 !== 0) {
                displayValue = current.toFixed(1);
            } else {
                displayValue = Math.floor(current);
            }
            
            counter.textContent = displayValue + suffix;
            requestAnimationFrame(updateCounter);
        };

        updateCounter();
    }

    // Intersection Observer to trigger animation when section is visible
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting && !hasAnimated) {
                hasAnimated = true;
                
                // Add staggered animation delay
                counters.forEach((counter, index) => {
                    setTimeout(() => {
                        counter.style.opacity = '1';
                        counter.style.transform = 'translateY(0)';
                        animateCounter(counter);
                    }, index * 200); // 200ms delay between each counter
                });
            }
        });
    }, {
        threshold: 0.5 // Trigger when 50% of the section is visible
    });

    // Initialize counters with opacity 0 and transform
    counters.forEach(counter => {
        counter.style.opacity = '0';
        counter.style.transform = 'translateY(20px)';
        counter.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
    });

    // Start observing the stats section
    if (statsSection) {
        observer.observe(statsSection);
    }

    // Fallback: If Intersection Observer is not supported
    if (!window.IntersectionObserver) {
        setTimeout(() => {
            counters.forEach((counter, index) => {
                setTimeout(() => {
                    counter.style.opacity = '1';
                    counter.style.transform = 'translateY(0)';
                    animateCounter(counter);
                }, index * 200);
            });
        }, 1000);
    }
});