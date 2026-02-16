// src/components/ui/Input.jsx
import React from 'react';

const Input = ({
    label,
    type = 'text',
    placeholder,
    value,
    onChange,
    error,
    icon: Icon,
    className = '',
    ...props
}) => {
    return (
        <div className={`w-full ${className}`}>
            {label && (
                <label className="block text-sm font-medium text-gray-300 mb-2">
                    {label}
                </label>
            )}
            <div className="relative">
                {Icon && (
                    <div className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400">
                        <Icon size={20} />
                    </div>
                )}
                <input
                    type={type}
                    value={value}
                    onChange={onChange}
                    placeholder={placeholder}
                    className={`
            w-full px-4 py-3 rounded-2xl
            bg-white/5 backdrop-blur-sm
            border border-white/10
            text-white placeholder-gray-500
            focus:outline-none focus:ring-2 focus:ring-[#A8E6CF] focus:border-transparent
            transition-all duration-300
            ${Icon ? 'pl-12' : ''}
            ${error ? 'border-red-500' : ''}
          `}
                    {...props}
                />
            </div>
            {error && (
                <p className="mt-2 text-sm text-red-400">{error}</p>
            )}
        </div>
    );
};

export default Input;
