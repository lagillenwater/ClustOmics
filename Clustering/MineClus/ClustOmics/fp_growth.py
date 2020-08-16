#%%

from score import Score
import copy

class FpGrowth:
    min_support = 0
    beta = 0.0
    best_score = Score()
    best_dimensions = set([])
    show_outputs = False

    
    def get_frequent_items(self, transactions):
        item_support_map = {}
        
        for tid, items in transactions.items():
            for item in items:
                if item not in item_support_map.keys():
                    item_support_map[item] = 0
                item_support_map[item] += 1
        
        max_support = 0
        max_item = ''
        frequent_items = set([])
        for item, support in item_support_map.items():
            if support>max_support:
                max_support = support
                max_item = item
            if support >= self.min_support:
               frequent_items.add(item)
        
        return frequent_items, max_item, max_support



    def remove_infrequent_items(self, transactions, frequent_items):
        new_transactions = {}
        
        for tid, items in transactions.items():
            for item in items:
                if item in frequent_items:
                    if tid not in new_transactions.keys():
                        new_transactions[tid] = set([item])
                    else:
                        new_transactions[tid].add(item)
        
        return new_transactions



    def build_header_table(self, transactions):
        header_table = {}
        item_support = {}
        
        for tid, items in transactions.items():
            for item in items:
                if item not in header_table.keys():
                    header_table[item] = set([])
                if item not in item_support.keys():
                    item_support[item] = 0
            
                header_table[item].add(tid)
                item_support[item]+=1
               
        delete_items = []
        for item, support in item_support.items():
            if support < self.min_support:
                delete_items.append(item)
                
        for item in delete_items:
            del item_support[item]
            tids = header_table[item]
            for tid in tids:
                transactions[tid].remove(item)
            
            del header_table[item]
            
        return header_table, item_support
    


    def is_path(self, transactions, ordered_items):
        if len(transactions) <= 1:
            return True  
        
        min_length = 1
        for item in ordered_items:
            
            for tid, items in transactions.items():
                if len(items) < min_length:
                    continue
                
                if item not in items:
                    return False
                
            min_length += 1
        
        return True

  

    def get_projected_transactions(self, transactions, tid_list, conditional_item):
        conditional_transactions = {}
        
        for tid in tid_list:
            transactions[tid].remove(conditional_item)
            if (len(transactions[tid])==0):
                del transactions[tid]
                
            else:
                conditional_transactions[tid] = transactions[tid].copy()
        
        return transactions, conditional_transactions  
 
    
 
    def update_best_score(self, new_best_score, new_dimensions):
        if self.show_outputs:
            print(f'*** score updated ***\n')
            print(f'old best score: {self.best_score}, dimensions:{self.best_dimensions}\n')
        self.best_dimensions = copy.deepcopy(new_dimensions)
        self.best_score = Score(new_best_score.dimension_count, new_best_score.support)
        
        if self.show_outputs:
            print(f'new best score: {new_best_score}, dimensions:{new_dimensions}\n')
            print('**************')



    def generate_itemsets(self, transactions, conditional_path, path_support, ordered_items, header_table):
        current_path = copy.deepcopy(conditional_path)
        current_support = path_support
        current_score = Score(len(current_path), current_support)
        
        if current_score.is_higher_than(self.best_score, self.beta):
            self.update_best_score(current_score, current_path)
            self.best_dimensions = copy.deepcopy(current_path)
        
        for item in ordered_items:
            current_path.append(item)
            current_support = len(header_table[item])
            current_score = Score(len(current_path), current_support)

            if current_score.is_higher_than(self.best_score, self.beta):
                self.update_best_score(current_score, current_path)
                self.best_dimensions = copy.deepcopy(current_path)



    def fp_growth(self, transactions, conditional_path, path_support):
        if self.show_outputs:
            print(f'conditional path: {conditional_path}, support: {path_support}\n')
            print(f'transaction: {transactions}\n')
        # build header table and remove infrequent items
        header_table, item_support_map = self.build_header_table(transactions)

        dimensions_upper_bound = len(conditional_path) + len(header_table)
        support_upper_bound = path_support
        score_upper_bound = Score(dimensions_upper_bound, support_upper_bound)

        if score_upper_bound.is_zero():
            return

        ordered_items = sorted(item_support_map.items(), key=lambda x: x[1], reverse=True)
        ordered_items = [x[0] for x in ordered_items]
        
        if self.show_outputs:
            print(f'header table: {header_table}\n')
            print(f'items support: {item_support_map}\n')
            print(f'ordered_items: {ordered_items}\n')
    
    
        # check if it's a single path
        if self.is_path(transactions, ordered_items):
            self.generate_itemsets(transactions, conditional_path, path_support, ordered_items, header_table)
            if self.show_outputs:
                print('is path')
                print(transactions)
                print('--------------------')
            
        else:
            # for each item in reverse order of frequency
            # update transactions
            # and recursively call fp-growth
            for item in reversed(ordered_items):
              
                transactions, cond_transactions = self.get_projected_transactions(transactions, header_table[item], item)
                if self.show_outputs:
                    print(f'projected transactions for {item}: {transactions}\n')
                    print(f'cond transactions for {item}: {cond_transactions}\n')
                new_conditional_path = copy.deepcopy(conditional_path)
                new_conditional_path.append(item)
                new_path_support = len(header_table[item])
                if self.show_outputs:
                    print('\n\n\n')
                self.fp_growth(copy.deepcopy(cond_transactions), new_conditional_path, new_path_support)
    


    def mine(self, transactions, min_support, beta, overall_best_score):
        self.min_support = min_support
        self.beta = beta
        self.best_score = Score(overall_best_score.dimension_count, overall_best_score.support)
        
        frequent_item_support, max_item, max_support = self.get_frequent_items(transactions)
        best_score = Score(1, max_support)
        self.update_best_score(best_score, [max_item])

        if self.show_outputs:
            print(f'initial best score: {self.best_score}')

        transactions = self.remove_infrequent_items(transactions, frequent_item_support)
        #header_table, item_support = self.build_header_table(transactions)
        #ordered_items = sorted(item_support.items(), key=lambda x: x[1], reverse=True)
        #ordered_items = [x[0] for x in ordered_items]

        self.fp_growth(transactions, [], 0)
         
        return self.best_score, self.best_dimensions
    
    


def load_transactions(path): 
    transactions = {}
    with open(path) as reader:
            file = reader.readlines()
            for line in file:
                values = [item.strip() for item in line.split(',')]
                transaction_id = values[0]
                transactions[transaction_id] = set(values[1:])
                
    return transactions



if __name__ == "__main__":
    transactions = load_transactions('dataset.csv')
    fp = FpGrowth()
    fp.mine(transactions, 3, 0.25, Score())
    
   

# %%
