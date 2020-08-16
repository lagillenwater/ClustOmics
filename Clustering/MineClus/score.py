import math

class Score:
    dimension_count = 0
    support = 0
    
    
    def __str__(self):
        return f'dimension_count:{self.dimension_count}, support: {self.support}'
        
 
    def __init__(self, dimension_count=0, support=0):
        self.dimension_count = dimension_count
        self.support = support
        
    
    def is_higher_than(self, other_score, beta):
        # this score has more dimensions and points -> return true
        if self.dimension_count >= other_score.dimension_count and self.support >= other_score.support:
            return True
        
        # this score has lower dimensions and points -> return false
        if self.dimension_count<other_score.dimension_count and self.support<other_score.support:
            return False
        
        delta_d = abs(self.dimension_count - other_score.dimension_count)
        
        # if this score has more dimensions but fewer points
        if self.dimension_count >= other_score.dimension_count:
            c_factor = other_score.support/self.support
            min_d = math.log(c_factor, 1/beta)
            return delta_d >= min_d
          
        c_factor = self.support/other_score.support
        min_d = math.log(c_factor, 1/beta)
        return delta_d < min_d


    def is_zero(self):
        return self.dimension_count == 0 and self.support == 0