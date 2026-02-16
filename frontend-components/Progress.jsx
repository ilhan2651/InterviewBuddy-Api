// src/components/ui/Progress.jsx
import React from 'react';

const Progress = ({
    value = 0,
    max = 100,
    label,
    showPercentage = true,
    color = 'primary',
    className = ''
}) => {
    const percentage = Math.round((value / max) * 100);

    const colors = {
        primary: 'bg-gradient-to-r from-[#A8E6CF] to-[#8FD9B6]',
        secondary: 'bg-gradient-to-r from-[#DCD6F7] to-[#C8BFE7]',
        success: 'bg-gradient-to-r from-green-400 to-green-500',
        warning: 'bg-gradient-to-r from-orange-400 to-orange-500',
        danger: 'bg-gradient-to-r from-red-400 to-red-500'
    };

    return (
        <div className={`w-full ${className}`}>
            {(label || showPercentage) && (
                <div className="flex justify-between items-center mb-2">
                    {label && <span className="text-sm font-medium text-gray-300">{label}</span>}
                    {showPercentage && <span className="text-sm font-semibold text-white">{percentage}%</span>}
                </div>
            )}
            <div className="w-full h-3 bg-white/10 rounded-full overflow-hidden backdrop-blur-sm">
                <div
                    className={`h-full ${colors[color]} transition-all duration-500 ease-out rounded-full`}
                    style={{ width: `${percentage}%` }}
                >
                    <div className="w-full h-full bg-white/20 animate-pulse"></div>
                </div>
            </div>
        </div>
    );
};

export default Progress;
