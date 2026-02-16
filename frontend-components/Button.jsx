// src/components/ui/Button.jsx
import React from 'react';

const Button = ({ 
  children, 
  variant = 'primary', 
  size = 'md', 
  onClick, 
  disabled = false,
  className = '',
  ...props 
}) => {
  const baseStyles = "font-semibold rounded-2xl transition-all duration-300 flex items-center justify-center gap-2";
  
  const variants = {
    primary: "bg-gradient-to-r from-[#A8E6CF] to-[#8FD9B6] text-[#1A1A2E] hover:shadow-lg hover:shadow-[#A8E6CF]/50 hover:scale-105",
    secondary: "bg-gradient-to-r from-[#DCD6F7] to-[#C8BFE7] text-[#1A1A2E] hover:shadow-lg hover:shadow-[#DCD6F7]/50 hover:scale-105",
    outline: "border-2 border-[#A8E6CF] text-[#A8E6CF] hover:bg-[#A8E6CF] hover:text-[#1A1A2E]",
    ghost: "text-gray-300 hover:bg-white/10",
    danger: "bg-gradient-to-r from-red-500 to-red-600 text-white hover:shadow-lg hover:shadow-red-500/50"
  };
  
  const sizes = {
    sm: "px-4 py-2 text-sm",
    md: "px-6 py-3 text-base",
    lg: "px-8 py-4 text-lg"
  };
  
  return (
    <button
      onClick={onClick}
      disabled={disabled}
      className={`${baseStyles} ${variants[variant]} ${sizes[size]} ${disabled ? 'opacity-50 cursor-not-allowed' : ''} ${className}`}
      {...props}
    >
      {children}
    </button>
  );
};

export default Button;
