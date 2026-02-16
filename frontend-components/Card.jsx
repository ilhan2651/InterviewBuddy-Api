// src/components/ui/Card.jsx
import React from 'react';

const Card = ({
    children,
    className = '',
    glass = false,
    hover = false,
    ...props
}) => {
    const glassStyles = glass
        ? "bg-white/10 backdrop-blur-lg border border-white/20"
        : "bg-gradient-to-br from-[#252540] to-[#1A1A2E] border border-white/10";

    const hoverStyles = hover ? "hover:shadow-2xl hover:shadow-[#A8E6CF]/20 hover:scale-[1.02] transition-all duration-300" : "";

    return (
        <div
            className={`rounded-3xl p-6 ${glassStyles} ${hoverStyles} ${className}`}
            {...props}
        >
            {children}
        </div>
    );
};

export default Card;
