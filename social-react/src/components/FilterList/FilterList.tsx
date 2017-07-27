import * as React from 'react';
import { connect, Dispatch } from 'react-redux';
import { FilterListItem, actionCreators, KnownAction } from '../../store/FilterList';
import { ApplicationState } from '../../store';

// At runtime, Redux will merge together...
type FilterListProps =
    { items: FilterListItem[] }
    & typeof actionCreators;

function FilterList({ items, requestFilterList }: FilterListProps) {
    items = items || new Array<FilterListItem>();
    return <div>
        <div>Filters</div>
        <div>
            <ul>
                {items.map((filter, index) => <li key={index}>{filter.name} - {filter.unReadNum}</li>)}
            </ul>
            <button onClick={requestFilterList}>reload</button>
        </div>
    </div>
}

function mapStateToProps(state: ApplicationState) {
    return {
        items: state.filterList
    }
}

function mapDispatchToProps(dispatch: Dispatch<KnownAction>) {
    return {
        requestFilterList: () => dispatch(actionCreators.requestFilterList())
    }
}

export default connect(mapStateToProps, mapDispatchToProps)(FilterList);