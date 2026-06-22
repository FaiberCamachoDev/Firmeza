import { motion } from 'framer-motion';
import type { CSSProperties, ReactNode } from 'react';

const variants = {
  initial: { opacity: 0, y: 14 },
  animate: { opacity: 1, y: 0, transition: { duration: 0.28, ease: 'easeOut' as const } },
  exit:    { opacity: 0, y: -8, transition: { duration: 0.18, ease: 'easeIn' as const } },
};

interface Props {
  children: ReactNode;
  className?: string;
  style?: CSSProperties;
}

export default function PageWrapper({ children, className = '', style }: Props) {
  return (
    <motion.div variants={variants} initial="initial" animate="animate" exit="exit" className={className} style={style}>
      {children}
    </motion.div>
  );
}
